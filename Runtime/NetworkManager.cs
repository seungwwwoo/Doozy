// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Text;
// using BestHTTP;
// using LitJson;
// using UnityEngine;
//
// namespace CoreManager
// {
//     public class NetworkManager : Singleton<NetworkManager>
//     {
//         [SerializeField] private string url = "https://testdev.pickiss.com:7106/";
//         [SerializeField] private string kevinUrl = "https://testdev.pickiss.com:7603/";
//         private readonly List<string> _listNetwork = new();
//         private int _failCount;
//         private bool _isFailMessageShow;
//         private int _downloadFailCount;
//         private bool _isDownloadFailMessageShow = true;
//
//         #region 기타 통신 RequestGamebaseLaunching, UpdateEpisodeStartRecord, UpdateEpisodeCompleteRecord
//
//         public void UpdateUserSelectionRecord(
//             int targetSceneID, int selectionGroup, int selectionNo, string selectionData)
//         {
//             JsonData sending = new JsonData
//             {
//                 ["episode_id"] = StoryManager.Instance.currentEpisodeID,
//                 ["target_scene_id"] = targetSceneID,
//                 ["selection_data"] = selectionData,
//                 ["selection_group"] = selectionGroup,
//                 ["selection_no"] = selectionNo,
//                 ["func"] = "updateUserSelectionProgress"
//             };
//
//             SendPatch(StoryManager.Instance.CallbackUpdateUserSelectionRecord, sending);
//         }
//
//
//         /// <summary>
//         /// 작품의 플레이 지점을 저장한다 (작품 순번 체크와 이어하기에서 사용)
//         /// </summary>
//         public void UpdateUserProjectCurrent(int episodeID, int sceneID, long scriptNo, bool showLoadingBar)
//         {
//             if (!UserManager.Instance.useRecord)
//                 return;
//
//             JsonData sending = new JsonData();
//             sending["project_id"] = StoryManager.Instance.currentProjectID; // 현재 프로젝트 
//             sending["episode_id"] = episodeID;
//             sending["scene_id"] = sceneID;
//             sending["script_no"] = scriptNo;
//             sending["func"] = "updateUserProject"; // func 지정 
//
//             SendPost(StoryManager.Instance.CallbackUpdateProjectCurrent, sending, showLoadingBar);
//         }
//
//
//         /// <summary>
//         /// visual collection 해금 처리
//         /// </summary>
//         public bool UpdateVisualCollection(int resourceID, string visualType)
//         {
//             if (!StoryManager.Instance.CheckVisualCollectionLock(resourceID, visualType))
//             {
//                 Debug.Log($"VisualCollection에 해당하지 않음 [{resourceID}]/[{visualType}]");
//                 return false;
//             }
//
//             JsonData sending = new JsonData
//             {
//                 ["resource_id"] = resourceID,
//                 ["visual_type"] = visualType,
//                 ["func"] = "addGalleryIllust"
//             };
//
//             SendPost((req, res) =>
//             {
//                 if (!CheckResponseValidation(req, res) || !string.IsNullOrEmpty(CheckResponseResult(res.DataAsText)))
//                 {
//                     Debug.LogError("VisualCollection 해금 실패");
//                     return;
//                 }
//
//                 JsonData responseData = JsonMapper.ToObject(res.DataAsText);
//                 StoryManager.Instance.UnlockVisualCollection(
//                     SystemManager.Instance.GetJsonNodeInt(responseData, "resource_id"),
//                     SystemManager.Instance.GetJsonNodeString(responseData, "visual_type")
//                 );
//             }, sending);
//             return true;
//         }
//
//         #endregion
//
//         public void GetUnreadMailList(OnRequestFinishedDelegate cb = null, bool showLoadingBar = true)
//         {
//             OnRequestFinishedDelegate callback = UserManager.Instance.CallbackNotReceiveMail;
//
//             if (cb != null)
//             {
//                 callback += cb;
//             }
//
//             JsonData sendingData = new JsonData
//             {
//                 ["func"] = "getUserMailList"
//             };
//
//             SendPost(callback, sendingData, showLoadingBar, showLoadingBar);
//         }
//
//         void SetBaseParams(JsonData jsonData)
//         {
//             jsonData["userkey"] = UserManager.Instance.userKey;
//             jsonData["build"] = Application.identifier;
//             jsonData["country"] = "XX";
//             jsonData["lang"] = SystemManager.Instance.currentAppLanguageCode;
//             jsonData["os"] = "Android";
//             jsonData["project_id"] = SystemManager.Instance.packageProjectID;
//             jsonData["ver"] = Application.version;
//             jsonData["localTime"] = DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss"));
//             jsonData["culture"] = UserManager.Instance.culture;
//
//
// #if UNITY_IOS
//             jsonData["os"] = "iOS"; // 아이폰
// #endif
//
//             if (Application.isEditor)
//             {
//                 jsonData["os"] = "Editor";
//             }
//         }
//
//         public void SendPatch(
//             OnRequestFinishedDelegate cb, JsonData sendingData, bool showLoadingBar = false, bool addList = true)
//         {
//             string requestURL = url + "client/";
//             sendingData ??= new JsonData();
//
//             SetBaseParams(sendingData);
//
//             if (sendingData.ContainsKey("func") && addList)
//             {
//                 _listNetwork.Add(sendingData["func"].ToString());
//             }
//
//             if (showLoadingBar)
//             {
//                 SystemManager.ShowNetworkLoading();
//             }
//             else
//             {
//                 SystemManager.HideNetworkLoading();
//             }
//
//             HTTPRequest request = new HTTPRequest(new Uri(requestURL), HTTPMethods.Patch, cb);
//
//             request.SetHeader("Content-Type", "application/json; charset=UTF-8");
//             request.RawData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(sendingData));
//
//             request.ConnectTimeout = TimeSpan.FromSeconds(10);
//             request.Timeout = TimeSpan.FromSeconds(15);
//             request.Tag = JsonMapper.ToJson(sendingData);
//             request.Send();
//         }
//
//         public void KevinSendPost(
//             OnRequestFinishedDelegate cb,
//             JsonData sendingData,
//             string path)
//         {
//             string requestURL = kevinUrl + path;
//             sendingData ??= new JsonData();
//             SetBaseParams(sendingData);
//
//             HTTPRequest request = new HTTPRequest(new Uri(requestURL), HTTPMethods.Post, cb);
//             request.SetHeader("Content-Type", "application/json; charset=UTF-8");
//             request.RawData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(sendingData));
//             request.ConnectTimeout = TimeSpan.FromSeconds(10);
//             request.Timeout = TimeSpan.FromSeconds(15);
//             request.Tag = JsonMapper.ToJson(sendingData);
//             request.Send();
//         }
//
//         public void SendPost(
//             OnRequestFinishedDelegate cb,
//             JsonData sendingData,
//             bool showLoadingBar = false,
//             bool addList = true)
//         {
//             string requestURL = url + "client/";
//             sendingData ??= new JsonData();
//             SetBaseParams(sendingData);
//
//             if (sendingData.ContainsKey("func") && addList)
//             {
//                 _listNetwork.Add(sendingData["func"].ToString());
//             }
//
//             if (showLoadingBar)
//             {
//                 SystemManager.ShowNetworkLoading();
//             }
//             else
//             {
//                 SystemManager.HideNetworkLoading();
//             }
//
//
//             HTTPRequest request = new HTTPRequest(new Uri(requestURL), HTTPMethods.Post, cb);
//             request.SetHeader("Content-Type", "application/json; charset=UTF-8");
//             request.RawData = Encoding.UTF8.GetBytes(JsonMapper.ToJson(sendingData));
//             request.ConnectTimeout = TimeSpan.FromSeconds(10);
//             request.Timeout = TimeSpan.FromSeconds(15);
//             request.Tag = JsonMapper.ToJson(sendingData);
//             request.Send();
//         }
//
//
//         public bool CheckResponseValidation(
//             HTTPRequest request, HTTPResponse response, bool dontHideNetworkLoading = false)
//         {
//             string message;
//             JsonData reqData = new();
//
//             #region ListNetwork에 대한 추가 처리
//
//             try
//             {
//                 if (request.RawData != null)
//                 {
//                     var requestRawData = Encoding.UTF8.GetString(request.RawData);
//                     requestRawData = requestRawData.Trim();
//
//                     if ((requestRawData.StartsWith("{") && requestRawData.EndsWith("}")) ||
//                         (requestRawData.StartsWith("[") && requestRawData.EndsWith("]")))
//                     {
//                         reqData = JsonMapper.ToObject(requestRawData);
//
//                         if (reqData != null && reqData.ContainsKey("func"))
//                         {
//                         }
//                     }
//                 }
//             }
//             catch (Exception e)
//             {
//                 Debug.Log(e.StackTrace);
//             }
//
//             #endregion
//
//             switch (request.State)
//             {
//                 case HTTPRequestStates.Finished:
//
//                     //통신이 실질적으로 완료됨.
//                     if (reqData != null && reqData.ContainsKey("func"))
//                     {
//                         Instance._listNetwork.Remove(reqData["func"].ToString());
//                     }
//
//
//                     // 일반적으로 네트워크 로딩 제거 
//                     if (CheckServerWork() && !dontHideNetworkLoading)
//                     {
//                         SystemManager.HideNetworkLoading();
//                     }
//
//                     // 실패 메세지가 보여지는 중이 아니라면 count 0으로 초기화
//                     if (!Instance._isFailMessageShow)
//                     {
//                         Instance._failCount = 0;
//                     }
//
//                     if (response.IsSuccess)
//                     {
//                         return true;
//                     }
//
//                     // 통신에는 성공했지만 status가 false 
//                     // 실패로 날아오는 경우는 message와 code가 날아온다. (서버에서)
//                     // 서버에서 status 400 으로 전달받는다. 
//                     message = getServerErrorMessage(response);
//
//
//                     if (!string.IsNullOrEmpty(message))
//                         SystemManager.Instance.ShowOneButtonMessagePopup(message);
//
//                     SystemManager.HideNetworkLoading();
//                     return false;
//
//                 // ! 여기서부터는 일반적인 응답 오류가 아닌 request 에러에 대한 처리 
//                 case HTTPRequestStates.Error: // 예상하지 못한 에러가 발생했다.
//                     message = SystemManager.Instance.GetLocalizedText("6173"); // 서버 오류가 발생함
//                     break;
//
//                 case HTTPRequestStates.Aborted: // 네트워크 연결이 끊어졌다. 
//                     message = SystemManager.Instance.GetLocalizedText("6174");
//                     break;
//
//                 case HTTPRequestStates.ConnectionTimedOut: // 서버에 연결되지 못함 
//                     message = SystemManager.Instance.GetLocalizedText("6175");
//                     break;
//
//                 case HTTPRequestStates.TimedOut: // 서버에서 주어진 시간내에 응답을 하지 못함
//                     message = SystemManager.Instance.GetLocalizedText("6176");
//                     break;
//
//                 default:
//                     message = "Unknown error occurred.";
//                     break;
//             }
//
//             // 팝업창으로 알려준다.
//             if (!string.IsNullOrEmpty(message))
//             {
//                 Instance._failCount++;
//                 if (!Instance._isFailMessageShow && Instance._failCount >= 5)
//                 {
//                     SystemManager.HideNetworkLoading();
//                     SystemManager.Instance.ShowOneButtonMessagePopup(message);
//                     return false;
//                 }
//
//                 request.Send();
//             }
//
//             return false;
//         }
//
//         public string CheckResponseResult(string resultData, bool showMessage = true)
//         {
//             JsonData checkingResultData;
//
//             try
//             {
//                 checkingResultData = JsonMapper.ToObject(resultData);
//             }
//             catch
//             {
//                 Debug.LogError("wrong result data");
//
//                 return string.Empty;
//             }
//
//             if (SystemManager.Instance.GetJsonNodeBool(checkingResultData, "result"))
//             {
//                 return string.Empty;
//             }
//
//             string messageID = SystemManager.Instance.GetJsonNodeString(checkingResultData, "messageID");
//             string devMessage = SystemManager.Instance.GetJsonNodeString(checkingResultData, "message");
//
//             Debug.Log($"CheckResponseResult [{messageID}]");
//
//             if (showMessage)
//             {
//                 SystemManager.Instance.ShowOneButtonMessagePopup(
//                     SystemManager.Instance.GetLocalizedText(messageID) + " [" + devMessage + "]");
//             }
//
//             return SystemManager.Instance.GetLocalizedText(messageID);
//         }
//
//         public bool CheckInGameDownloadValidation(HTTPRequest request, HTTPResponse response)
//         {
//             string exceptionMessage;
//
//             switch (request.State)
//             {
//                 case HTTPRequestStates.Finished:
//                     if (response.IsSuccess)
//                     {
//                         if (!Instance._isDownloadFailMessageShow)
//                         {
//                             Instance._downloadFailCount = 0;
//                         }
//
//                         return true;
//                     }
//
//                     // * AWS S3의 경우, 리소스가 없는 경우에 대해서는 request는 완료,
//                     // * response에서 fail을 준다. 
//                     // * 이 경우는 서버 설정에 따라 진입을 막을지 허용할지 처리한다. allow_missing_resource
//                     try
//                     {
//                         exceptionMessage = string.Format("{0}-{1} Response Message", response.StatusCode,
//                             response.Message);
//                     }
//                     catch
//                     {
//                         exceptionMessage = "finished but response is fail";
//                     }
//
//                     Debug.LogError($"Download response fail [{exceptionMessage}]");
//
//                     ReportRequestError(
//                         request.Uri.ToString(),
//                         $"Request is finished, but response failed [{response.StatusCode}]");
//
//                     break;
//
//
//                 default:
//                     exceptionMessage = request.State.ToString();
//                     Debug.LogError(string.Format("!!! Download request fail [{0}]", exceptionMessage));
//
//                     // 서버로 리포트 
//                     Instance.ReportRequestError(request.Uri.ToString(),
//                         string.Format("Request failed. [{0}]", request.State.ToString()));
//                     break;
//             }
//
//
//             // * 요청이 올바르게 처리되지 못한경우 exceptionMessage가 할당된다. 
//             // * 한번에 날아간 여러개의 다운로드 요청을 합해서 실패 카운트가 10 이상이면 로비로 되돌려보낸다. 
//             // * 실패 요청은 1초 후에 재시도 처리한다.
//             if (!string.IsNullOrEmpty(exceptionMessage))
//             {
//                 // 시간초과이거나, 다운받지 못한 경우... 
//                 Debug.LogError("Download Fail : " + exceptionMessage);
//                 Instance._downloadFailCount++;
//
//                 if (!Instance._isDownloadFailMessageShow && Instance._downloadFailCount >= 10)
//                 {
//                     SystemManager.HideNetworkLoading();
//
//                     SystemManager.Instance.ShowOneButtonMessagePopupLocalize("80084");
//                     Instance._isDownloadFailMessageShow = true; // 메세지 중복 호출 막는다. 
//                     return false;
//                 }
//
//                 // 요청 다시 시도한다.
//                 Instance.ResendRequest(request);
//             }
//
//             return false;
//         }
//
//         private void ResendRequest(HTTPRequest request)
//         {
//             StartCoroutine(RoutineResend(request));
//         }
//
//         IEnumerator RoutineResend(HTTPRequest request)
//         {
//             yield return new WaitForSeconds(1); // 1초 있다가 재시도 
//             request.Send();
//         }
//
//
//         /// <summary>
//         /// Request Error 리포트하기.(HTTPRequest Error 발생시에 메세지 처리하지 않고 서버로 리포트하기.) 
//         /// </summary>
//         public void ReportRequestError(string rawData, string message)
//         {
//             JsonData sendingData = new JsonData
//             {
//                 ["rawData"] = rawData,
//                 ["message"] = message,
//                 ["func"] = "reportRequestError"
//             };
//             SendPost((request, response) => CheckResponseValidation(request, response), sendingData, false, false);
//         }
//
//         private string getServerErrorMessage(HTTPResponse response)
//         {
//             if (string.IsNullOrEmpty(response.DataAsText))
//             {
//                 return "Failed to receive detailed error message.";
//             }
//
//             JsonData errorData;
//             string errorMessage;
//             string errorCode;
//             try
//             {
//                 errorData = JsonMapper.ToObject(response.DataAsText);
//
//                 if (errorData.ContainsKey("code"))
//                 {
//                     errorCode = errorData["code"].ToString();
//                     errorMessage = SystemManager.Instance.GetLocalizedText(errorCode);
//                 }
//                 else
//                 {
//                     errorMessage = "Unknwon message";
//                 }
//
//                 return errorMessage;
//             }
//             catch (Exception e)
//             {
//                 Debug.Log(e.StackTrace);
//                 errorMessage = "Unknown Exception";
//             }
//
//             return errorMessage;
//         }
//
//         public bool CheckServerWork()
//         {
//             return _listNetwork.Count <= 0;
//         }
//     }
// }