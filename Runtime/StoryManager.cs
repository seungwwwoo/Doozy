// using System.Collections.Generic;
// using System.Collections;
// using UnityEngine;
// using System.Linq;
// using System;
// using LitJson;
// using BestHTTP;
// using Doozy.Runtime.Signals;
// using UnityEngine.Serialization;
//
//
// namespace CoreManager
// {
//     public enum EpisodeType
//     {
//         Chapter,
//         Ending,
//         Side
//     }
//
//     // 에피소드 진행중에는 prev, current, future, block 에피소드가 엔딩에 들어온 경우 해당 엔딩 제외 전부 block처리
//     public enum EpisodeState
//     {
//         None,
//         Prev,
//         Current,
//         Future,
//         Block
//     };
//
//     public class StoryManager : Singleton<StoryManager>
//     {
//         public StoryData currentProject; // 현재 선택한 작품 Master 정보 
//         [NonSerialized] public bool IsDetailLoaded; // 디테일 정보 로딩 완료 여부 
//         private JsonData _projectDetail; // 조회로 가져온 작품에 대한 기준정보와 유저정보.
//         public EpisodeData currentEpisodeData;
//         public ProjectCurrentData regularProjectCurrent; // 정규 스토리 진도
//         private JsonData _episodeListJson; // 에피소드 리스트 JSON 
//         private JsonData _sideEpisodeListJson; // 사이드 에피소드 리스트 JSON 
//
//         private readonly List<EpisodeData> _regularEpisodeList = new(); // chapter(정규)만 따로 빼놓는다.
//         private readonly List<EpisodeData> _sideEpisodeList = new(); // 사이드 에피소드
//         public List<EpisodeData> listCurrentProjectEpisodes = new(); // 현재 선택된 혹은 플레이중인 작품의 EpisodeData의 List.
//         private static Action _onCleanUserEpisodeProgress; // 유저 에피소드 씬 진척도 클리어 콜백 
//
//         [Space] [Header("== 에피소드 카운팅 ==")] public int regularEpisodeCount; // 엔딩을 제외한 정규 에피소드 개수
//
//         // ReSharper disable once NotAccessedField.Global
//         public int totalEndingCount; // 엔딩 총 개수
//
//         // ReSharper disable once NotAccessedField.Global
//         public int unlockEndingCount; // 해금된 엔딩 개수
//
//         // ReSharper disable once NotAccessedField.Global
//         public int sideEpisodeCount; // 사이드 에피소드 개수
//
//         [Space]
//
//         // * 능력 관련 
//         // private JsonData _currentStoryAbilityJson; // 현재 선택한 작품의 능력치 Json
//         private Dictionary<string, List<AbilityData>> _dictStoryAbility; // 받아온 능력치 화자별로 딕셔너리 처리 
//
//         private List<AbilityData> _allAbilityData;
//
//         // * 프로필 
//         private List<ProfileData> _listProfile;
//         private Dictionary<string, ProfileData> _dictSpeakerProfile;
//         public Dictionary<string, ProfileData> DictSpeakerProfile => _dictSpeakerProfile;
//
//         // * 타이머 관련 처리 (쓰는 프로젝트도 있고, 안쓰는 프로젝트도 있음)
//         [Header("타이머 리워드")] private JsonData _timerRewardJson; // 오토메 리워드 카운트 정보 
//         public int timerRewardCount;
//         public int timerAdRewardCount;
//
//         #region 말풍선 세트와 관련된 JSON , 변수
//
//         private JsonData _bubbleSetJson; // 말풍선 세트 (마스터)
//         private JsonData _talkBubbleJson; // 대화 말풍선 기준정보
//         private JsonData _whisperBubbleJson; // 속삭임 말풍선 기준정보
//         private JsonData _feelingBubbleJson; // 속마음 말풍선 기준정보
//         private JsonData _yellBubbleJson; // 외침 말풍선 기준정보
//         private JsonData _monologueBubbleJson; // 독백 말풍선 기준정보
//         private JsonData _speechBubbleJson; // 중요대사 말풍선 기준정보
//
//         public Dictionary<string, string> BubbleIDDictionary; // 말풍선 id-url 집합 
//         public Dictionary<string, string> BubbleURLDictionary; // 말풍선 url-key 집합 
//         private Dictionary<string, string> _bubbleIDDictionaryForLobby; // 말풍선 id-url 집합 (로비-꾸미기)
//
//         // ReSharper disable once CollectionNeverQueried.Local
//         private Dictionary<string, string> _bubbleURLDictionaryForLobby; // 말풍선 url-key 집합 (로비-꾸미기)
//         private JsonData _storyNameTag;
//         private readonly Dictionary<string, NametagData> _dictNameTag = new();
//
//         #endregion
//
//         #region 선택한 작품의 라이브 리소스 모델 데이터
//
//         private JsonData _modelJson; // 프로젝트 모델 JSON 
//         private Dictionary<string, JsonData> _dictProjectModel; // 프로젝트 모델의 Dictionary (캐릭터이름 - Json 조합)
//
//         private Dictionary<string, List<LiveResourceData>>
//             _dictProjectLiveIllust; // 라이브 일러스트 Dictionary 일러스트이름 - JSON 조합
//
//         private Dictionary<string, List<LiveResourceData>>
//             _dictProjectLiveObject; // 라이브 오브젝트 Dictionary 오브젝트이름 - JSON 조합
//
//         #endregion
//
//         #region 선택한 작품의 기준정보 데이터
//
//         private JsonData _dressJson; // 의상 정보
//         private List<DressData> _listProjectDress; // 작품의 모든 의상정보 
//         private Dictionary<string, List<DressData>> _dictProjectSpeakerDress; // 작품의 캐릭터별 의상정보 
//         private JsonData _profileDialogueJson; // 프로필 대사 정보 
//         private Dictionary<string, List<ProfileLineData>> _dictProfileDialogue; // 프로필상 유저 대사 라인 
//         public JsonData LoadingJson; // 로딩 스크린 정보
//         public JsonData LoadingDetailJson; // 로딩 디테일 정보 
//         private JsonData _itemListJson; // 아이템 정보
//         public List<ItemData> allItemDataList;
//         public List<ItemData> inventoryItemList; // 인벤토리 아이템 
//         private Dictionary<string, List<ItemData>> _dictSpeakerItems; // 캐릭터별 아이템 정보 Dict. 
//         private Dictionary<string, LobbyCostumeData> _dictLobbyCostume; // 캐릭터별 입고 있는 복장 정보 
//         public List<DlcMasterData> listDlc;
//         public AdventureData adventureData; // 유저의 현재 모험 정보
//         private List<VisualCollectionData> _listVisualCollection = new(); // Visual 수집요소
//         private readonly List<StaticImageData> _listIllustImage = new(); // 일러스트 이미지 전체
//         private readonly List<StaticImageData> _listMinicutImage = new(); // 일러스트 이미지 전체
//         private readonly List<StaticImageData> _listBackgroundImage = new(); // 배경 이미지 전체
//         [Space] private readonly List<SideUnlockData> _listSideUnlock = new(); // 사이드 언락 정보 
//
//         #endregion
//
//         [FormerlySerializedAs("CurrentProjectID")]
//         public int currentProjectID = -1; // 선택한 프로젝트 ID 
//
//         [FormerlySerializedAs("CurrentEpisodeID")]
//         public int currentEpisodeID = -1; // 선택한 에피소드 ID 
//
//         // * 말풍선 정보 
//         // ReSharper disable once NotAccessedField.Local
//         [SerializeField] private int currentBubbleSetID = 50; // 현재 말풍선 세트 ID 
//         [SerializeField] private int currentBubbleSetVersion; // 현재 말풍선세트 버전 정보 
//         private JsonData _currentBubbleSetJson;
//         public JsonData CurrentBubbleMasterJson; // 선택한 프로젝트 버블 마스터 정보 
//
//         #region static, const
//
//         private const string KeyBubbleDetailPrefix = "BubbleDetail";
//         private const string KeyBubbleVerPrefix = "BubbleVer";
//         private const string NodeBubbleMaster = "bubbleMaster"; // 말풍선 마스터 노드 
//         private const string NodeNameTag = "nametag"; // 네임태그 노드 
//         private const string NodeProjectModels = "models"; // 프로젝트의 모든 모델 파일리스트 
//
//         // 말풍선 관련 컬럼들 추가 
//         private const string BubbleTalk = "talk";
//         private const string BubbleWhisper = "whisper";
//         private const string BubbleFeeling = "feeling";
//         private const string BubbleYell = "yell";
//         private const string BubbleMonologue = "monologue"; // 독백 
//         private const string BubbleSpeech = "speech"; // 중요대사
//
//         public const string BubbleVariationNormal = "normal";
//         public const string BubbleVariationEmoticon = "emoticon";
//         public const string BubbleVariationReverseEmoticon = "reverse_emoticon";
//         public const string BubbleVariationDouble = "double"; // 2인스탠딩 배리에이션 
//
//         #endregion
//
//
//         public void GetUserAbility()
//         {
//             JsonData sending = new JsonData
//             {
//                 [CommonConst.FUNC] = "requestUserAbility"
//             };
//
//             NetworkManager.Instance.SendPost((_, response) =>
//             {
//                 Debug.Assert(response.IsSuccess && !string.IsNullOrEmpty(response.DataAsText), "GetUserAbility Error");
//
//                 JsonData result = JsonMapper.ToObject(response.DataAsText);
//
//                 Debug.Assert(result.ContainsKey(GameConst.TEMPLATE_ABILITY), "OnFinishedRequestUserAbility Error");
//
//                 result = result[GameConst.TEMPLATE_ABILITY];
//                 SetStoryAbilityDictionary(result, true);
//             }, sending, false, false);
//         }
//
//
//         void CallbackStoryInfo(HTTPRequest req, HTTPResponse res)
//         {
//             if (!NetworkManager.Instance.CheckResponseValidation(req, res, true))
//             {
//                 Debug.LogError("Network Error in CallbackStoryInfo");
//                 return;
//             }
//
//             if (!string.IsNullOrEmpty(NetworkManager.Instance.CheckResponseResult(res.DataAsText)))
//             {
//                 return;
//             }
//
//             Debug.Log("#### Callback Story Info START");
//             _projectDetail = JsonMapper.ToObject(res.DataAsText);
//             StartCoroutine(EnteringStory()); // 코루틴으로 변경 2022.02.10
//         }
//
//         /// <summary>
//         /// 정규 스토리 진행정보 설정
//         /// </summary>
//         private void setRegularProjectCurrent(JsonData j)
//         {
//             Debug.Assert(j != null, "setRegularProjectCurrent Error");
//
//             if (regularProjectCurrent == null || !regularProjectCurrent.isValidData)
//             {
//                 regularProjectCurrent = new ProjectCurrentData(j);
//             }
//             else
//             {
//                 regularProjectCurrent.UpdateData(j);
//             }
//
//             currentEpisodeID = regularProjectCurrent.episode_id;
//             currentEpisodeData = GetRegularEpisodeByID(currentEpisodeID);
//
//             if (currentEpisodeData == null || !currentEpisodeData.isValidData)
//             {
//                 SystemManager.Instance.ShowDebugErrorMessage(
//                     "Invalid current main story episode : " + currentEpisodeID, "SetRegularProjectCurrent");
//                 return;
//             }
//
//
//             refreshRegularEpisodePlayState();
//         }
//
//         /// <summary>
//         /// 정규 스토리 진행 에피
//         /// </summary>
//         /// <returns></returns>
//         public int GetRegularProjectCurrentEpiosdeID()
//         {
//             if (regularProjectCurrent == null || !regularProjectCurrent.isValidData)
//             {
//                 // Debug.LogError("Invalid state in GetRegularProjectCurrentEpiosdeID");
//                 return 0;
//             }
//
//             return regularProjectCurrent.episode_id;
//         }
//
//
//         /// <summary>
//         /// 패키지 프로젝트 정보 및 유저 정보 조회, 작품정보 불러오기 (마스터, 디테일)
//         /// </summary>
//         public void RequestPackageStoryInfo()
//         {
//             _episodeListJson = null;
//             _storyNameTag = null;
//             _sideEpisodeListJson = null;
//             _projectDetail = null;
//             currentProjectID = -1;
//             listCurrentProjectEpisodes.Clear();
//             LoadBubbleSetLocalInfo();
//
//             JsonData sending = new JsonData
//             {
//                 [CommonConst.FUNC] = "getPackageStoryInfo",
//                 [CommonConst.COL_PROJECT_ID] = SystemManager.Instance.packageProjectID,
//                 ["userBubbleVersion"] = currentBubbleSetVersion, // 로컬에 저장된 말풍선 버전 정보 
//                 [LobbyConst.COL_LANG] = SystemManager.Instance.currentAppLanguageCode
//             };
//
//             // NetworkManager.Instance.SendPost((req, res) =>
//             // {
//             //     Debug.Assert(res.IsSuccess && !string.IsNullOrEmpty(res.DataAsText), "RequestPackageStoryInfo Error");
//             //     _projectDetail = JsonMapper.ToObject(res.DataAsText);
//             //     StartCoroutine(EnteringStory());
//             // }, sending, true);
//             //
//             NetworkManager.Instance.SendPost(CallbackStoryInfo, sending);
//             // NetworkManager.Instance.SendPost((req, res) =>
//             // {
//             //     Debug.Assert(res.IsSuccess && !string.IsNullOrEmpty(res.DataAsText), "RequestPackageStoryInfo Error");
//             //     _projectDetail = JsonMapper.ToObject(res.DataAsText);
//             //     StartCoroutine(EnteringStory());
//             // }, sending, true);
//         }
//
//         /// <summary>
//         /// ! 로비에 진입하며, 불러온 정보들을 세팅합니다. 
//         /// </summary>
//         /// <returns></returns>        
//         IEnumerator EnteringStory()
//         {
//             // 로딩 이미지 다운로드 요청
//             // DownloadProjectAllEpisodeLoading();
//
//             // 뚝뚝 끊기는걸 방지하기 위해 프레임을 나눈다. 
//             yield return null;
//
//             // StoryData (프로젝트 마스터 정보)
//             if (_projectDetail.ContainsKey("project"))
//             {
//                 currentProject = new StoryData(_projectDetail["project"]);
//                 currentProjectID = currentProject.projectID;
//                 SystemManager.Instance.givenStoryData = currentProject;
//             }
//
//             Debug.Log("### EnteringStory : StoryData Create Done");
//
//
//             // 공지사항
//             if (_projectDetail != null && _projectDetail.ContainsKey(CommonConst.COL_NOTICE))
//             {
//                 SystemManager.Instance.SetNoticeData(_projectDetail[CommonConst.COL_NOTICE]);
//                 Debug.Log("### EnteringStory : Notice Create Done");
//             }
//
//             initEpisodeList();
//             InitGameBubble();
//             yield return null;
//             SetBubbles();
//             yield return null;
//             // 프로젝트 네임태그 
//             SetProjectNameTag();
//             // 프로젝트 모델, 라이브 일러스트, 오브젝트 설정
//             SetProjectModels();
//             SetProjectLiveIllusts();
//             SetProjectLiveObjects();
//             yield return null;
//             SetProjectStaticImages();
//             SetVisualCollection();
//
//             // * 프로젝트 프로필 (대사, 아이템, 프로필, 의상)
//             if (_projectDetail.ContainsKey(CommonConst.NODE_PROFILE))
//             {
//                 SetProjectProfile(_projectDetail[CommonConst.NODE_PROFILE]);
//             }
//             else
//             {
//                 Debug.LogError("No Profile DATA");
//             }
//
//
//             // project current 
//             if (_projectDetail.ContainsKey(CommonConst.NODE_PROJECT_CURRENT))
//                 setRegularProjectCurrent(_projectDetail[CommonConst.NODE_PROJECT_CURRENT]);
//             else
//             {
//                 Debug.LogError("정규 스토리 진행정보 없음!");
//             }
//
//
//             yield return null;
//
//             // 사이드(스페셜) 에피소드 언락정보 2024.02.02
//             // * 꼭 Item, Ability, Episode 다 초기화된 상태에서 호출할것.
//             InitSideUnlock();
//
//
//             // TODO 로딩 이미지 처리 방식 변경 필요
//
//             // 완료했으면 loadingJson, loaindgDetailJson 재할당.
//             // loadingJson은 처음에는 다운로드를 위해 episodeLoadingList 노드를 사용했지만 
//             // 그 후에는 loading 노드를 사용한다. 
//             LoadingJson = SystemManager.Instance.GetJsonNode(_projectDetail, GameConst.NODE_LOADING);
//             LoadingDetailJson = SystemManager.Instance.GetJsonNode(_projectDetail, GameConst.NODE_LOADING_DETAIL);
//
//             // 
//             Debug.Log("<color=cyan>Story detail done</color>");
//             IsDetailLoaded = true;
//
//             Signal.Send(LobbyConst.STREAM_IFYOU, "moveStoryLoading", "open!");
//
//             // DLC 정보 초기화
//             InitDlc();
//
//             // * 프로젝트에서는 타이머와 어드벤처 중에 하나를 사용함. 
//
//             // 어드벤처 정보 가져오기 
//             InitAdvneture();
//
//             // 타이머 리워드 관련 
//             SetTimerRewardCount();
//         }
//
//         /// <summary>
//         /// 타이머 초기화
//         /// </summary>
//         void SetTimerRewardCount()
//         {
//             if (_projectDetail.ContainsKey("timerReward"))
//             {
//                 _timerRewardJson = _projectDetail["timerReward"];
//
//
//                 timerAdRewardCount = SystemManager.Instance.GetJsonNodeInt(_timerRewardJson, "ad_reward_count");
//                 timerRewardCount = SystemManager.Instance.GetJsonNodeInt(_timerRewardJson, "timer_reward_count");
//             }
//             else
//             {
//                 Debug.Log("No timer reward node in ProjectDetailJson");
//             }
//         }
//
//         /// <summary>
//         /// 사이드 언락정보 초기화 
//         /// </summary>
//         void InitSideUnlock()
//         {
//             if (!_projectDetail.ContainsKey(CommonConst.NODE_SIDE_UNLOCK))
//                 return;
//
//             JsonData unlockInfo = _projectDetail[CommonConst.NODE_SIDE_UNLOCK];
//             _listSideUnlock.Clear();
//
//             for (int i = 0; i < unlockInfo.Count; i++)
//             {
//                 SideUnlockData data = new SideUnlockData(unlockInfo[i]);
//                 _listSideUnlock.Add(data);
//             }
//         }
//
//
//         /// <summary>
//         /// 말풍선 초기화 
//         /// </summary>
//         void InitGameBubble()
//         {
//             try
//             {
//                 if (_projectDetail.ContainsKey(NodeBubbleMaster))
//                 {
//                     // 말풍선의 경우 데이터가 많아서, 버전 관리를 통해서 신규 버전이 있을때만 서버에서 내려받도록 합니다.     
//                     // 말풍선 세트 버전관리 
//
//                     // 말풍선 마스터 정보 
//                     CurrentBubbleMasterJson = _projectDetail[NodeBubbleMaster];
//
//                     currentBubbleSetID =
//                         SystemManager.Instance.GetJsonNodeInt(CurrentBubbleMasterJson, "bubbleID"); // 연결된 말풍선 세트 ID
//                     currentBubbleSetVersion =
//                         SystemManager.Instance.GetJsonNodeInt(CurrentBubbleMasterJson, "bubble_ver"); // 말풍선 버전 
//                 }
//
//                 // 말풍선 기초정보가 없는 경우, Local의 정보를 불러와서 설정 
//                 if (_projectDetail.ContainsKey(CommonConst.NODE_BUBBLE_SET))
//                 {
//                     // 말풍선 정보 있으면, 로컬에 저장하기
//                     _currentBubbleSetJson = _projectDetail[CommonConst.NODE_BUBBLE_SET];
//
//                     ES3.Save(KeyBubbleVerPrefix, currentBubbleSetVersion);
//                     ES3.Save(KeyBubbleDetailPrefix, JsonMapper.ToStringUnicode(_currentBubbleSetJson));
//                 }
//                 else
//                 {
//                     // 말풍선 정보 없는 경우는 로컬에서 로드하여 노드에 할당 
//                     _projectDetail[CommonConst.NODE_BUBBLE_SET] = _currentBubbleSetJson;
//                 }
//             }
//             catch (Exception e)
//             {
//                 NetworkManager.Instance.ReportRequestError(e.StackTrace,
//                     $"Bubble Master [{currentProjectID}]");
//             }
//
//             Debug.Log("### EnteringStory : Loading bubble json done");
//         }
//
//
//         private void initEpisodeList()
//         {
//             EpisodeData newEpisodeData;
//
//             _sideEpisodeList.Clear();
//
//             _regularEpisodeList.Clear();
//             regularEpisodeCount = 0; // 정규 에피소드 카운트 
//             unlockEndingCount = 0; // 해금된 엔딩 수 
//             totalEndingCount = 0; // 전체 엔딩의 개수 
//
//
//             // 
//             _episodeListJson = _projectDetail[CommonConst.NODE_EPISODE]; // 프로젝트의 정규 에피소드 (챕터 + 엔딩)
//             _sideEpisodeListJson = _projectDetail[CommonConst.NODE_SIDE]; // 프로젝트의 사이드 에피소드
//             sideEpisodeCount = _sideEpisodeListJson.Count; // 사이드 에피소드 전체 
//
//
//             // 정규 에피소드 (chatper + ending)
//             for (int i = 0; i < _episodeListJson.Count; i++)
//             {
//                 newEpisodeData = new EpisodeData(_episodeListJson[i]);
//
//                 listCurrentProjectEpisodes.Add(newEpisodeData); // 정규+엔딩 모아주기 
//
//                 if (SystemManager.Instance.GetJsonNodeBool(_episodeListJson[i], "ending_open"))
//                     unlockEndingCount++;
//
//
//                 if (SystemManager.Instance.GetJsonNodeString(_episodeListJson[i], LobbyConst.STORY_EPISODE_TYPE) !=
//                     CommonConst.COL_CHAPTER)
//                 {
//                     totalEndingCount++;
//                     continue;
//                 }
//
//                 _regularEpisodeList.Add(newEpisodeData);
//             }
//
//             regularEpisodeCount = _regularEpisodeList.Count; // 카운팅 
//
//
//             // * 사이드 에피소드는 리스트 마지막에 넣어주자.
//             for (int i = 0; i < _sideEpisodeListJson.Count; i++)
//             {
//                 newEpisodeData = new EpisodeData(_sideEpisodeListJson[i]);
//
//                 listCurrentProjectEpisodes.Add(newEpisodeData); // 모든 에피소드 
//                 _sideEpisodeList.Add(newEpisodeData); // 사이드만 모아주기 
//             }
//
//             Debug.Log($"<color=yellow>[{_episodeListJson.Count}] Episodes are loaded </color>");
//         } // ? InitEpisodeList
//
//
//         #region Visual Collection , SideUnlock
//
//         /// <summary>
//         /// speaker의 능력을 조건으로 갖고있는 unlock 정보 주세요!
//         /// </summary>
//         public SideUnlockData FindSpeakerSideUnlock(AbilityData ability)
//         {
//             foreach (var sideUnlock in _listSideUnlock)
//             {
//                 if (sideUnlock.unlock_type == "ability"
//                     && sideUnlock.TargetAbility != null
//                     && sideUnlock.TargetAbility == ability)
//                 {
//                     return sideUnlock;
//                 }
//             }
//
//             return null;
//         }
//
//         public List<SideUnlockData> GetAbilitySideuUnlocks()
//         {
//             return _listSideUnlock.Where(unlock => unlock.unlock_type == "ability").ToList();
//         }
//
//         /// <summary>
//         /// 프로젝트에 1개있는 어빌리티 올클리어 언락 정보 주세요!
//         /// </summary>
//         public SideUnlockData GetAllClearSideUnlock()
//         {
//             foreach (var sideUnlock in _listSideUnlock)
//             {
//                 if (sideUnlock.unlock_type == "allAbilityClear")
//                 {
//                     return sideUnlock;
//                 }
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 모든아이템 수집 사이드 언락정보 
//         /// </summary>
//         /// <returns></returns>
//         public SideUnlockData GetItemCollectSideUnlock()
//         {
//             for (int i = 0; i < _listSideUnlock.Count; i++)
//             {
//                 if (_listSideUnlock[i].unlock_type == "allItem")
//                 {
//                     return _listSideUnlock[i];
//                 }
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 비주얼 해금요소 세팅 
//         /// </summary>
//         void SetVisualCollection()
//         {
//             if (!_projectDetail.ContainsKey(CommonConst.NODE_VISUAL_COLLECTION))
//             {
//                 return;
//             }
//
//             _listVisualCollection = new List<VisualCollectionData>();
//
//             for (int i = 0; i < _projectDetail[CommonConst.NODE_VISUAL_COLLECTION].Count; i++)
//             {
//                 _listVisualCollection.Add(
//                     new VisualCollectionData(_projectDetail[CommonConst.NODE_VISUAL_COLLECTION][i]));
//             }
//         } // ? END SetVisualCollection
//
//         public List<VisualCollectionData> GetVisualCollection()
//         {
//             return _listVisualCollection;
//         }
//
//         /// <summary>
//         /// 대상 DLC의 visual collection 리스트 받아오기 
//         /// </summary>
//         public List<VisualCollectionData> GetVisualCollectionsForDlc(int dlcID)
//         {
//             return _listVisualCollection.Where(item => item.dlc_id == dlcID).ToList();
//         }
//
//         /// <summary>
//         /// 대상 비주얼 수집요소를 아직 획득하지 않았는지 체크
//         /// </summary>
//         public bool CheckVisualCollectionLock(int resourceID, string type)
//         {
//             // 조건이 같고, has_collection이 false이면 잠겨있는 상태다!
//             return _listVisualCollection.Where(item =>
//                            item.resource_id == resourceID && item.visual_type == type && !item.has_collection).ToList()
//                        .Count >
//                    0;
//         }
//
//         /// <summary>
//         /// 해금처리! 
//         /// </summary>
//         public void UnlockVisualCollection(int resourceID, string type)
//         {
//             Debug.Log($"😀 UnlockVisualCollection : {resourceID} / {type}");
//
//             foreach (var visualCollection in _listVisualCollection)
//             {
//                 if (visualCollection.resource_id == resourceID
//                     && visualCollection.visual_type == type)
//                 {
//                     visualCollection.has_collection = true; // 해금 처리 
//                 }
//             }
//         }
//
//         /// <summary>
//         /// 해당 이름에 해당하는 수집요소가 있는지 체크 
//         /// </summary>
//         public bool CheckVisualCollectionExists(string resourceName)
//         {
//             return _listVisualCollection.Where(item => item.resource_name == resourceName).ToList().Count > 0;
//         }
//
//         public VisualCollectionData FindVisualCollection(int resourceID, string type)
//         {
//             return _listVisualCollection.Find(item => item.resource_id == resourceID && item.visual_type == type);
//         }
//
//         #endregion
//
//         #region 프로젝트 프로필 (Ability, Profile Line이 생성된 상태에서 실행필요! 순서가 중요)
//
//         /// <summary>
//         /// 프로젝트 프로필 생성 
//         /// </summary>
//         public void SetProjectProfile(JsonData profileJson, bool needRefresh = true)
//         {
//             if (needRefresh)
//             {
//                 SetProjectAbility();
//                 SetProfileLine();
//                 SetProjectItems(); // Item 에서 Ability를 끌어쓰기 때문에 순서가 중요.
//                 SetProjectDress(); // 프로젝트 의상
//             }
//
//             _listProfile = new List<ProfileData>();
//             _dictSpeakerProfile = new Dictionary<string, ProfileData>();
//
//             if (profileJson == null || profileJson.Count == 0)
//             {
//                 Debug.LogError("No Profile DATA");
//                 return;
//             }
//
//
//             for (int i = 0; i < profileJson.Count; i++)
//             {
//                 ProfileData profile = new ProfileData(profileJson[i]);
//
//                 _listProfile.Add(profile);
//                 _dictSpeakerProfile[profile.speaker] = profile;
//             }
//
//             SetLobbySpeakerDress(); // 로비 캐릭터들의 의상 
//         } // ? END SetProjectProfile
//
//         // 프로필 리프레시 
//         public void RefreshProfile()
//         {
//             for (int i = 0; i < _listProfile.Count; i++)
//             {
//                 _listProfile[i].InitAbility();
//             }
//         }
//
//         public ProfileData GetSpeakerProfile(string speaker)
//         {
//             if (_dictSpeakerProfile == null || !_dictSpeakerProfile.ContainsKey(speaker))
//                 return null;
//
//             return _dictSpeakerProfile[speaker];
//         }
//
//         /// <summary>
//         /// 프로젝트 의상 정보 설정 
//         /// </summary>
//         void SetProjectDress()
//         {
//             if (!_projectDetail.ContainsKey(CommonConst.NODE_DRESS))
//             {
//                 Debug.LogError("의상 정보 없음!");
//                 return;
//             }
//
//             _dressJson = _projectDetail[CommonConst.NODE_DRESS];
//
//             _listProjectDress = new List<DressData>();
//             _dictProjectSpeakerDress = new Dictionary<string, List<DressData>>();
//
//             // JSON => Class & 수집
//             for (int i = 0; i < _dressJson.Count; i++)
//             {
//                 DressData dress = new DressData(_dressJson[i]);
//
//                 _listProjectDress.Add(dress);
//                 if (!_dictProjectSpeakerDress.ContainsKey(dress.speaker))
//                 {
//                     _dictProjectSpeakerDress.Add(dress.speaker, new List<DressData>());
//                 }
//
//                 _dictProjectSpeakerDress[dress.speaker].Add(dress);
//             }
//         }
//
//
//         DressData GetSpeakerDefaultDress(string speaker)
//         {
//             if (!_dictProjectSpeakerDress.ContainsKey(speaker))
//                 return null;
//
//             foreach (DressData dress in _dictProjectSpeakerDress[speaker])
//             {
//                 if (dress.isDefault)
//                     return dress;
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 대상 캐릭터의 모든 의상정보
//         /// </summary>
//         public List<DressData> GetSpeakerAllDress(string speaker)
//         {
//             if (!_dictProjectSpeakerDress.ContainsKey(speaker))
//                 return null;
//
//             return _dictProjectSpeakerDress[speaker];
//         }
//
//         /// <summary>
//         /// 주인공 캐릭터들이 로비에서 입고 있는 복장 정보 생성 
//         /// </summary>
//         void SetLobbySpeakerDress()
//         {
//             _dictLobbyCostume = new Dictionary<string, LobbyCostumeData>();
//
//             foreach (string key in _dictSpeakerProfile.Keys)
//             {
//                 if (!_dictLobbyCostume.ContainsKey(key))
//                 {
//                     // 없는 경우에만!
//                     DressData defaultDress = GetSpeakerDefaultDress(key);
//                     if (defaultDress == null || !defaultDress.isValidData)
//                     {
//                         Debug.LogError($"{key} 에게 기본의상이 없음!");
//                         continue;
//                     }
//
//                     // 디폴트 드레스를 기반으로 복장 생성.
//                     _dictLobbyCostume.Add(key, new LobbyCostumeData(defaultDress));
//                 }
//             }
//
//             // userDress 정보와 연동
//             if (!_projectDetail.ContainsKey(CommonConst.NODE_USER_DRESS))
//             {
//                 Debug.LogError("유저 드레스 정보가 없음..!");
//                 return;
//             }
//
//             // 연동 시작. 
//             JsonData userDressJson = _projectDetail[CommonConst.NODE_USER_DRESS];
//             for (int i = 0; i < userDressJson.Count; i++)
//             {
//                 string speaker = SystemManager.Instance.GetJsonNodeString(userDressJson[i], "speaker");
//                 int dressID = SystemManager.Instance.GetJsonNodeInt(userDressJson[i], "dress_id");
//                 bool isMain = SystemManager.Instance.GetJsonNodeBool(userDressJson[i], "is_main");
//
//                 if (_dictLobbyCostume.ContainsKey(speaker))
//                 {
//                     DressData newDress = _listProjectDress.Find(item => item.dress_id == dressID);
//                     if (newDress == null || !newDress.isValidData)
//                     {
//                         Debug.LogError($"유저 복장 정보와 연결된 dress_id가 올바르지 않음 {speaker}/{dressID}");
//                         continue;
//                     }
//
//                     _dictLobbyCostume[speaker].SetNewDress(newDress);
//                     _dictLobbyCostume[speaker].isMain = isMain;
//                 }
//             }
//         }
//
//         #endregion
//
//         #region 프로젝트 능력 Ability 관련 메소드 모음
//
//         /// <summary>
//         /// 작품 능력 정보 설정 
//         /// </summary>
//         private void SetProjectAbility()
//         {
//             if (_projectDetail.ContainsKey(CommonConst.NODE_USER_ABILITY))
//             {
//                 SetStoryAbilityDictionary(_projectDetail[CommonConst.NODE_USER_ABILITY]);
//             }
//             else
//             {
//                 Debug.LogError("### No Ability Data");
//             }
//         }
//
//         public void SetStoryAbilityDictionary(JsonData ability, bool shouldUpdate = false)
//         {
//             // * AbilityData는 다른 데이터 클래스 (Profile, Item)에서 참조를 걸고 있기 때문에 
//             // 로비에 진입할때가 아니면 new로 하지 않는다.(참조가 깨짐)
//             // 로비로 들어올때는 다 새로 만들기때문에 new 로 해도 상관없다. 
//
//             if (shouldUpdate)
//             {
//                 // Dict 갱신으로 진행한다. 
//                 foreach (string key in ability.Keys)
//                 {
//                     for (int i = 0; i < ability[key].Count; i++)
//                     {
//                         // 서버에서 받은 새로운 값 
//                         AbilityData newAbility = new AbilityData(ability[key][i]);
//
//                         // 기존에 들고있던 이전 ability 
//                         AbilityData previousAbility = GetAbilityDataByID(key, newAbility.abilityID);
//
//                         if (previousAbility != null && previousAbility.isValidData)
//                         {
//                             previousAbility.UpdateAbility(newAbility);
//                         }
//                     }
//                 }
//             }
//             else
//             {
//                 // 완전히 새로 생성
//                 // DictStoryAbility 를  새로 만든다.
//                 _dictStoryAbility = new Dictionary<string, List<AbilityData>>();
//                 _allAbilityData = new List<AbilityData>();
//
//                 foreach (string key in ability.Keys)
//                 {
//                     List<AbilityData> abilityDatas = new List<AbilityData>();
//
//                     for (int i = 0; i < ability[key].Count; i++)
//                     {
//                         AbilityData abilityData = new AbilityData(ability[key][i]);
//                         abilityDatas.Add(abilityData);
//                         _allAbilityData.Add(abilityData);
//                     }
//
//                     _dictStoryAbility.Add(key, abilityDatas);
//                 }
//             }
//         }
//
//         /// <summary>
//         /// 능력 찾아주세요..!
//         /// </summary>
//         public AbilityData GetAbilityDataByName(string speaker, string abilityName)
//         {
//             if (!_dictStoryAbility.ContainsKey(speaker))
//             {
//                 Debug.LogError(speaker + ", 이 캐릭터는 능력치 데이터에 없는걸");
//                 return null;
//             }
//
//             // 해당 캐릭터의 능력치 정보를 찾아서 return
//             for (int i = 0; i < _dictStoryAbility[speaker].Count; i++)
//             {
//                 if (_dictStoryAbility[speaker][i].originAbilityName == abilityName)
//                     return _dictStoryAbility[speaker][i];
//             }
//
//             Debug.LogError(string.Format("{0}, 이 캐릭터는 {1} 능력치가 존재하지 않습니다", speaker, abilityName));
//             return null;
//         }
//
//         public AbilityData GetAbilityDataByID(string speaker, int abilityID)
//         {
//             Debug.Log($"GetAbilityDataByID : [{speaker}]/[{abilityID}]");
//
//             if (!_dictStoryAbility.ContainsKey(speaker))
//             {
//                 Debug.LogError($"능력치 정보가 없음 {speaker} / {abilityID}");
//                 return null;
//             }
//
//             // 해당 캐릭터의 능력치 정보를 찾아서 return
//             for (int i = 0; i < _dictStoryAbility[speaker].Count; i++)
//             {
//                 if (_dictStoryAbility[speaker][i].abilityID == abilityID)
//                     return _dictStoryAbility[speaker][i];
//             }
//
//             Debug.LogError(string.Format("{0}, 이 캐릭터는 {1} 능력치가 존재하지 않습니다", speaker, abilityID));
//             return null;
//         }
//
//         /// <summary>
//         /// ID로만 찾기 
//         /// </summary>
//         public AbilityData GetAbilityDataByID(int abilityID)
//         {
//             foreach (var ability in _allAbilityData)
//             {
//                 if (ability.abilityID == abilityID)
//                 {
//                     return ability;
//                 }
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 캐릭터별 능력 리스트 전체 
//         /// </summary>
//         public List<AbilityData> GetSpeakerAbilityList(string speaker)
//         {
//             return !_dictStoryAbility.ContainsKey(speaker) ? null : _dictStoryAbility[speaker];
//         }
//
//         #endregion
//
//
//         /// <summary>
//         /// 프로젝트 아이템 
//         /// </summary>
//         void SetProjectItems()
//         {
//             _itemListJson = SystemManager.Instance.GetJsonNode(_projectDetail, "items");
//
//             if (_itemListJson == null)
//             {
//                 _dictSpeakerItems = null;
//                 return;
//             }
//
//             _dictSpeakerItems = new Dictionary<string, List<ItemData>>();
//             allItemDataList = new List<ItemData>();
//             inventoryItemList = new List<ItemData>();
//
//
//             for (int i = 0; i < _itemListJson.Count; i++)
//             {
//                 string speaker = SystemManager.Instance.GetJsonNodeString(_itemListJson[i], "speaker");
//
//                 if (!string.IsNullOrEmpty(speaker) && !_dictSpeakerItems.ContainsKey(speaker))
//                 {
//                     _dictSpeakerItems.Add(speaker, new List<ItemData>());
//                 }
//
//                 ItemData newItem = new ItemData(_itemListJson[i]);
//
//                 // 인벤토리 아이템 리스트에 추가 
//                 if (newItem.item_type == "key" || newItem.item_type == "gift")
//                 {
//                     if (!inventoryItemList.Contains(newItem))
//                         inventoryItemList.Add(newItem);
//                 }
//
//                 // 리스트에도 넣어주고, 딕셔너리에도 넣어준다.
//                 allItemDataList.Add(newItem);
//
//                 // speaker 정보 있는 경우만!
//                 if (!string.IsNullOrEmpty(speaker))
//                     _dictSpeakerItems[speaker].Add(newItem);
//             }
//
//             // 인벤토리 아이템 썸네일 이미지 셋팅 
//             for (int i = 0; i < inventoryItemList.Count; i++)
//             {
//                 if (inventoryItemList[i].iconSprite == null)
//                     inventoryItemList[i].SetItemSprite();
//             }
//
//             foreach (string key in _dictSpeakerItems.Keys)
//             {
//                 Debug.Log($"#ITEM# : {key} has {_dictSpeakerItems[key].Count} items");
//             }
//
//             // // 드레스커스컴 (현재 복장정보 업데이트 해주기)
//             // UpdateSpeakerCurrentDress(UserManager.main.dressCustomJSON);
//
//             Debug.Log("<color=white>### SetProjectItems Done</color>");
//         }
//
//         /// <summary>
//         /// 유저의 아이템 리스트 업데이트
//         /// </summary>
//         public void UpdateProjectItems(List<AdventureRewardData> rewardDatas)
//         {
//             foreach (var reward in rewardDatas)
//             {
//                 foreach (var item in allItemDataList)
//                 {
//                     if (item.item_id != reward.item_id) continue;
//                     item.hasItem = true;
//                     item.hasItemCount += reward.quantity;
//                     break;
//                 }
//             }
//         }
//
//         /// <summary>
//         /// 대상 아이템의 현재 수량값을 업데이트한다. 
//         /// </summary>
//         public void UpdateProjectItems(int itemID, int quantity)
//         {
//             foreach (var itemData in allItemDataList)
//             {
//                 if (itemData.item_id == itemID)
//                 {
//                     itemData.hasItem = quantity > 0;
//                     itemData.hasItemCount = quantity;
//                     itemData.CheckItemEffect();
//                     break;
//                 }
//             }
//         }
//
//
//         /// <summary>
//         /// 대상 캐릭터의 아이템 목록 받기.
//         /// </summary>
//         public List<ItemData> GetSpeakerItems(string speaker)
//         {
//             if (!_dictSpeakerItems.TryGetValue(speaker, out var speakerItems))
//                 return null;
//
//             return speakerItems;
//         }
//
//         /// <summary>
//         /// ID로 아이템 찾기 
//         /// </summary>
//         public ItemData GetItemDataByID(int itemID)
//         {
//             return allItemDataList.Find(item => item.item_id == itemID);
//         }
//
//         public ItemData GetItemDataByOriginName(string originName)
//         {
//             return allItemDataList.Find(item => item.origin_name == originName);
//         }
//
//         /// <summary>
//         /// 아이템 (의상) 소유 정보 업데이트 
//         /// </summary>
//         public void UpdateItemOwnership(JsonData propertyData)
//         {
//             if (propertyData == null || propertyData.Count == 0)
//             {
//                 return;
//             }
//
//             for (int i = 0; i < propertyData.Count; i++)
//             {
//                 int itemID = SystemManager.Instance.GetJsonNodeInt(propertyData[i], "item_id");
//                 int quantity = SystemManager.Instance.GetJsonNodeInt(propertyData[i], "current_quantity");
//                 UpdateProjectItems(itemID, quantity);
//             }
//         }
//
//
//         /// <summary>
//         /// 로비에 서있을 메인 캐릭터 복장 정보 가져오기 
//         /// </summary>
//         public LobbyCostumeData GetMainLobbyCostume()
//         {
//             if (_dictLobbyCostume == null)
//             {
//                 Debug.LogError("### DictSpeakerDress is not ready");
//                 return null;
//             }
//
//             foreach (string key in _dictLobbyCostume.Keys)
//             {
//                 if (_dictLobbyCostume[key].isMain)
//                 {
//                     return _dictLobbyCostume[key];
//                 }
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 로비 메인 캐릭터 speaker 
//         /// </summary>
//         public string GetMainLobbyCostumeSpeaker()
//         {
//             if (_dictLobbyCostume == null)
//             {
//                 Debug.LogError("### DictSpeakerDress is not ready");
//                 return null;
//             }
//
//             foreach (string key in _dictLobbyCostume.Keys)
//             {
//                 if (_dictLobbyCostume[key].isMain)
//                     return key;
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 메인으로 지정된 로비 스탠딩 캐릭터의 복장정보 조회 
//         /// </summary>
//         public LobbyCostumeData GetLobbyCostume(string speaker)
//         {
//             if (_dictLobbyCostume.TryGetValue(speaker, out var costume))
//             {
//                 return costume;
//             }
//
//             Debug.LogError($"### DictSpeakerDress key [{speaker}] is missing");
//             return null;
//         }
//
//         /// <summary>
//         /// 메인 스탠딩 캐릭터 설정 
//         /// </summary>
//         public void SetNewMainLobbySpeakerCostume(string speaker)
//         {
//             foreach (var key in _dictLobbyCostume.Keys)
//             {
//                 _dictLobbyCostume[key].isMain = key == speaker;
//             }
//         }
//
//         public void ChangeLobbySpeakerCostume(string speaker, DressData newDress)
//         {
//             Debug.Assert(_dictLobbyCostume.ContainsKey(speaker), $"### DictSpeakerDress key [{speaker}] is missing");
//
//             if (!_dictLobbyCostume.ContainsKey(speaker))
//             {
//                 Debug.LogError("로비코스튬 정보 없음!!!");
//                 return;
//             }
//
//             _dictLobbyCostume[speaker].SetNewDress(newDress);
//         }
//
//
//         /// <summary>
//         /// 에피소드 아이디를 가지고 에피소드 찾기 
//         /// </summary>
//         public EpisodeData FindEpisode(int episodeID)
//         {
//             foreach (var episode in _regularEpisodeList)
//             {
//                 if (episode.episodeID == episodeID)
//                 {
//                     return episode;
//                 }
//             }
//
//             foreach (var episode in _sideEpisodeList)
//             {
//                 if (episode.episodeID == episodeID)
//                 {
//                     return episode;
//                 }
//             }
//
//             foreach (var episode in listCurrentProjectEpisodes)
//             {
//                 // 위에서 정규도 사이드도 아니었으니 엔딩이 아니면 스킵
//                 if (episode.episodeType != EpisodeType.Ending)
//                 {
//                     continue;
//                 }
//
//                 if (episode.episodeID == episodeID)
//                 {
//                     return episode;
//                 }
//             }
//
//             return null;
//         }
//
//         #region 네임태그 관련 메소드
//
//         /// <summary>
//         /// 프로젝트 네임태그 설정 
//         /// </summary>
//         private void SetProjectNameTag()
//         {
//             _dictNameTag.Clear();
//
//             if (!_projectDetail.ContainsKey(NodeNameTag))
//             {
//                 Debug.Log("Nametag Info is null");
//                 _storyNameTag = null;
//                 return;
//             }
//
//             _storyNameTag = _projectDetail[NodeNameTag];
//
//             // speaker 컬럼별로 분류!
//             for (int i = 0; i < _storyNameTag.Count; i++)
//             {
//                 NametagData tagData = new NametagData(_storyNameTag[i]);
//                 _dictNameTag.TryAdd(tagData.speaker, tagData);
//             }
//         }
//
//         /// <summary>
//         /// 네임태그의 언어별 캐릭터 이름 
//         /// </summary>
//         public string GetNameTagName(string speaker)
//         {
//             if (string.IsNullOrEmpty(speaker))
//             {
//                 return string.Empty;
//             }
//
//             return !_dictNameTag.TryGetValue(speaker, out var value)
//                 ? speaker
//                 : value.GetLocalizedName(SystemManager.Instance.currentAppLanguageCode);
//         }
//
//         /// <summary>
//         /// 네임태그의 색상정보 알려주세요
//         /// </summary>
//         public string GetNametagColor(string speaker, bool isMainColor = true)
//         {
//             if (string.IsNullOrEmpty(speaker))
//             {
//                 return string.Empty;
//             }
//
//             if (!_dictNameTag.ContainsKey(speaker))
//             {
//                 return GameConst.COLOR_BLACK_RGB;
//             }
//
//             return isMainColor ? _dictNameTag[speaker].main_color : _dictNameTag[speaker].sub_color;
//         }
//
//         #endregion
//
//
//         /// <summary>
//         /// 프로젝트 static 이미지 설정 
//         /// </summary>
//         private void SetProjectStaticImages()
//         {
//             try
//             {
//                 _listIllustImage.Clear();
//                 _listMinicutImage.Clear();
//                 _listBackgroundImage.Clear();
//
//                 // 일러스트 
//                 if (_projectDetail.ContainsKey(CommonConst.NODE_PROJECT_ILLUSTS))
//                 {
//                     for (int i = 0; i < _projectDetail[CommonConst.NODE_PROJECT_ILLUSTS].Count; i++)
//                     {
//                         StaticImageData imageData =
//                             JsonUtility.FromJson<StaticImageData>(
//                                 JsonMapper.ToJson(_projectDetail[CommonConst.NODE_PROJECT_ILLUSTS][i]));
//                         _listIllustImage.Add(imageData);
//                     }
//                 }
//
//                 // 미니컷
//                 if (_projectDetail.ContainsKey(CommonConst.NODE_PROJECT_MINICUTS))
//                 {
//                     for (int i = 0; i < _projectDetail[CommonConst.NODE_PROJECT_MINICUTS].Count; i++)
//                     {
//                         StaticImageData imageData =
//                             JsonUtility.FromJson<StaticImageData>(
//                                 JsonMapper.ToJson(_projectDetail[CommonConst.NODE_PROJECT_MINICUTS][i]));
//                         _listMinicutImage.Add(imageData);
//                     }
//                 }
//
//                 // 배경
//                 if (_projectDetail.ContainsKey(CommonConst.NODE_PROJECT_BACKGROUNDS))
//                 {
//                     for (int i = 0; i < _projectDetail[CommonConst.NODE_PROJECT_BACKGROUNDS].Count; i++)
//                     {
//                         StaticImageData imageData =
//                             JsonUtility.FromJson<StaticImageData>(
//                                 JsonMapper.ToJson(_projectDetail[CommonConst.NODE_PROJECT_BACKGROUNDS][i]));
//                         _listBackgroundImage.Add(imageData);
//                     }
//                 }
//
//                 Debug.Log(
//                     $"###### Static Image Count : {_listIllustImage.Count}/{_listMinicutImage.Count}/{_listBackgroundImage.Count}");
//             }
//             catch (Exception e)
//             {
//                 NetworkManager.Instance.ReportRequestError(e.StackTrace,
//                     string.Format("SetProjectStandard [{0}]", currentProjectID));
//             }
//         }
//
//         /// <summary>
//         /// 프로필 대사 수집 
//         /// </summary>
//         private void SetProfileLine()
//         {
//             if (!_projectDetail.ContainsKey(CommonConst.NODE_PROFILE_LINE))
//             {
//                 Debug.LogError("#### NO profileLineJSON in SetProfileLine #####");
//                 return;
//             }
//
//             _profileDialogueJson = _projectDetail[CommonConst.NODE_PROFILE_LINE];
//
//             // 캐릭터별 대사 수집 
//             _dictProfileDialogue = new Dictionary<string, List<ProfileLineData>>();
//
//
//             foreach (string key in _profileDialogueJson.Keys)
//             {
//                 List<ProfileLineData> lines = new List<ProfileLineData>();
//
//
//                 for (int i = 0; i < _profileDialogueJson[key].Count; i++)
//                 {
//                     ProfileLineData profileLineData = new ProfileLineData(_profileDialogueJson[key][i]);
//                     lines.Add(profileLineData);
//                 }
//
//                 // 캐릭터별로 분류
//                 _dictProfileDialogue.Add(key, lines);
//                 Debug.Log($"{key} has {lines.Count} lines");
//             }
//         }
//
//         public List<ProfileLineData> GetProfileLine(string speaker)
//         {
//             return _dictProfileDialogue.GetValueOrDefault(speaker);
//         }
//
//         /// <summary>
//         /// 프로젝트 라이브 오브젝트 설정
//         /// </summary>
//         void SetProjectLiveObjects()
//         {
//             try
//             {
//                 if (_projectDetail.ContainsKey(CommonConst.NODE_PROJECT_LIVE_OBJECTS))
//                 {
//                     _dictProjectLiveObject = new Dictionary<string, List<LiveResourceData>>();
//
//                     foreach (string key in _projectDetail[CommonConst.NODE_PROJECT_LIVE_OBJECTS].Keys)
//                     {
//                         // LiveResourceData liveData = JsonUtility.FromJson<LiveResourceData>()            
//                         // 리소스 이름 (key) 에 묶여서 목록이 날아온다.             
//                         JsonData jsonData = _projectDetail[CommonConst.NODE_PROJECT_LIVE_OBJECTS][key];
//                         List<LiveResourceData> list = new List<LiveResourceData>();
//
//                         for (int i = 0; i < jsonData.Count; i++)
//                         {
//                             // 각 목록을 LiveResourceData로 만든다. 
//                             LiveResourceData liveData =
//                                 JsonUtility.FromJson<LiveResourceData>(JsonMapper.ToJson(jsonData[i]));
//                             list.Add(liveData);
//                         }
//
//                         _dictProjectLiveObject.Add(key, list);
//                         Debug.Log($"라이브오브젝트 [{key}] 추가됨!");
//                     }
//                 }
//                 else
//                 {
//                     Debug.LogError("라이브 오브젝트 정보가 상세정보에 없음");
//                 }
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError(e.StackTrace);
//                 NetworkManager.Instance.ReportRequestError(e.StackTrace,
//                     string.Format("SetProjectLiveObjects [{0}]", currentProjectID));
//             }
//         }
//
//
//         /// <summary>
//         /// 프로젝트 라이브 일러스트 설정
//         /// </summary>
//         void SetProjectLiveIllusts()
//         {
//             try
//             {
//                 if (_projectDetail.ContainsKey(CommonConst.NODE_PROJECT_LIVE_ILLUSTS))
//                 {
//                     _dictProjectLiveIllust = new Dictionary<string, List<LiveResourceData>>();
//
//                     foreach (string key in _projectDetail[CommonConst.NODE_PROJECT_LIVE_ILLUSTS].Keys)
//                     {
//                         // LiveResourceData liveData = JsonUtility.FromJson<LiveResourceData>()            
//                         // 리소스 이름 (key) 에 묶여서 목록이 날아온다.             
//                         JsonData jsonData = _projectDetail[CommonConst.NODE_PROJECT_LIVE_ILLUSTS][key];
//                         List<LiveResourceData> list = new List<LiveResourceData>();
//
//                         for (int i = 0; i < jsonData.Count; i++)
//                         {
//                             // 각 목록을 LiveResourceData로 만든다. 
//                             LiveResourceData liveData =
//                                 JsonUtility.FromJson<LiveResourceData>(JsonMapper.ToJson(jsonData[i]));
//                             list.Add(liveData);
//                         }
//
//                         _dictProjectLiveIllust.Add(key, list);
//                     }
//                 }
//             }
//             catch (Exception e)
//             {
//                 Debug.LogError(e.StackTrace);
//                 NetworkManager.Instance.ReportRequestError(e.StackTrace,
//                     string.Format("SetProjectLiveIllusts [{0}]", currentProjectID));
//             }
//         }
//
//
//         /// <summary>
//         /// 프로젝트 모델 정보 설정 
//         /// </summary>
//         void SetProjectModels()
//         {
//             try
//             {
//                 // 프로젝트 모델 JSON 
//                 _modelJson = _projectDetail[NodeProjectModels];
//                 _dictProjectModel = new Dictionary<string, JsonData>();
//
//                 foreach (string key in _modelJson.Keys)
//                 {
//                     _dictProjectModel.Add(key, _modelJson[key]);
//                 }
//             }
//             catch (Exception e)
//             {
//                 NetworkManager.Instance.ReportRequestError(e.StackTrace, $"SetProjectModels [{currentProjectID}]");
//             }
//         }
//
//
//         /// <summary>
//         /// 말풍선 세트 정보 설정 
//         /// </summary>
//         void SetBubbles()
//         {
//             try
//             {
//                 _bubbleSetJson = GetNodeBubbleSet(); // 말풍선 세트만 따로 받아온다. 
//                 // 템플릿 6종!
//                 _talkBubbleJson = SystemManager.Instance.GetJsonNode(_bubbleSetJson, BubbleTalk); // 대화 
//                 _whisperBubbleJson = SystemManager.Instance.GetJsonNode(_bubbleSetJson, BubbleWhisper); // 속삭임
//                 _feelingBubbleJson = SystemManager.Instance.GetJsonNode(_bubbleSetJson, BubbleFeeling); // 속마음 
//                 _yellBubbleJson = SystemManager.Instance.GetJsonNode(_bubbleSetJson, BubbleYell); // 외침
//                 _monologueBubbleJson = SystemManager.Instance.GetJsonNode(_bubbleSetJson, BubbleMonologue); // 독백
//                 _speechBubbleJson = SystemManager.Instance.GetJsonNode(_bubbleSetJson, BubbleSpeech); // 중요대사
//
//
//                 CollectBubbleImages();
//             }
//             catch (Exception e)
//             {
//                 NetworkManager.Instance.ReportRequestError(e.StackTrace,
//                     $"SetBubbles [{currentProjectID}]");
//             }
//
//             Debug.Log("### EnteringStory : SetBubbles Done");
//         }
//
//         #region 작품 리소스 관리 일러스트, 미니컷, 라이브 일러스트, 라이브 오브제, 캐릭터 모델
//
//         public StaticImageData GetMiniCut(string cutName)
//         {
//             for (int i = 0; i < _listMinicutImage.Count; i++)
//             {
//                 if (_listMinicutImage[i].image_name == cutName)
//
//                     return _listMinicutImage[i];
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 일러스트 이름으로 일러스트 기준정보 찾기
//         /// </summary>
//         /// <param name="illustName">일러스트 명칭</param>
//         /// <returns>일러스트 id와 type을 담은 JsonData</returns>
//         public StaticImageData GetImageIllustData(string illustName)
//         {
//             // * 2021.12.23 이 메소드는 일반 일러스트만을 대상으로 합니다. 
//             // illust_id, image_name, image_url,key ,is_public, appear_episode, public_name, live_pair_id..
//             for (int i = 0; i < _listIllustImage.Count; i++)
//             {
//                 if (_listIllustImage[i].image_name == illustName)
//                     return _listIllustImage[i];
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 배경 데이터 찾기 (이름으로)
//         /// </summary>
//         public StaticImageData GetBackgroundImageData(string bgName)
//         {
//             foreach (var staticImageData in _listBackgroundImage)
//             {
//                 if (staticImageData.image_name == bgName)
//                     return staticImageData;
//             }
//
//             return null;
//         }
//
//         public StaticImageData GetBackgroundImageData(int id)
//         {
//             return _listBackgroundImage.Find(item => item.id == id);
//         }
//         
//
//         /// <summary>
//         /// 모델 이름으로 모델 리소스 정보 가져오기 
//         /// </summary>
//         public JsonData GetModelJsonByModelName(string modelName)
//         {
//             if (!_dictProjectModel.ContainsKey(modelName))
//                 return null;
//
//             return _dictProjectModel[modelName];
//         }
//
//         /// <summary>
//         /// 이름으로 라이브 일러스트 정보 가져오기 
//         /// </summary>
//         public List<LiveResourceData> GetLiveIllustDataByName(string resourceName)
//         {
//             if (!_dictProjectLiveIllust.ContainsKey(resourceName))
//                 return null;
//
//             return _dictProjectLiveIllust[resourceName];
//         }
//
//
//         /// <summary>
//         /// 이름으로 라이브 오브젝트 정보 가져오기 
//         /// </summary>
//         public List<LiveResourceData> GetLiveObjectDataByName(string resourceName)
//         {
//             if (!_dictProjectLiveObject.ContainsKey(resourceName))
//                 return null;
//
//             return _dictProjectLiveObject[resourceName];
//         }
//
//         #endregion
//
//
//         #region 말풍선 리소스
//
//         /// <summary>
//         /// 버블 이미지 수집하기 
//         /// </summary>
//         void CollectBubbleImages()
//         {
//             BubbleIDDictionary = new Dictionary<string, string>();
//             BubbleURLDictionary = new Dictionary<string, string>();
//
//             // * 꾸미기 용도의 Dict 추가 2022.04.
//             _bubbleIDDictionaryForLobby = new Dictionary<string, string>();
//             _bubbleURLDictionaryForLobby = new Dictionary<string, string>();
//
//
//             // 각 말풍선 그룹별로 처리 한다.
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_talkBubbleJson, BubbleVariationNormal),
//                 true); // 대화 템플릿만 미리 다운로드 필요
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_talkBubbleJson, BubbleVariationEmoticon));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_talkBubbleJson, BubbleVariationReverseEmoticon));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_talkBubbleJson, BubbleVariationDouble));
//
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_whisperBubbleJson, BubbleVariationNormal));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_whisperBubbleJson, BubbleVariationEmoticon));
//             CollectGroupImageInfo(
//                 SystemManager.Instance.GetJsonNode(_whisperBubbleJson, BubbleVariationReverseEmoticon));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_whisperBubbleJson, BubbleVariationDouble));
//
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_feelingBubbleJson, BubbleVariationNormal));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_feelingBubbleJson, BubbleVariationEmoticon));
//             CollectGroupImageInfo(
//                 SystemManager.Instance.GetJsonNode(_feelingBubbleJson, BubbleVariationReverseEmoticon));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_feelingBubbleJson, BubbleVariationDouble));
//
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_yellBubbleJson, BubbleVariationNormal));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_yellBubbleJson, BubbleVariationEmoticon));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_yellBubbleJson, BubbleVariationReverseEmoticon));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_yellBubbleJson, BubbleVariationDouble));
//
//
//             // 추가된 2종의 템플릿(독백과 중요대사)
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_monologueBubbleJson, BubbleVariationNormal));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_monologueBubbleJson, BubbleVariationEmoticon));
//             CollectGroupImageInfo(
//                 SystemManager.Instance.GetJsonNode(_monologueBubbleJson, BubbleVariationReverseEmoticon));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_monologueBubbleJson, BubbleVariationDouble));
//
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_speechBubbleJson, BubbleVariationNormal));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_speechBubbleJson, BubbleVariationEmoticon));
//             CollectGroupImageInfo(
//                 SystemManager.Instance.GetJsonNode(_speechBubbleJson, BubbleVariationReverseEmoticon));
//             CollectGroupImageInfo(SystemManager.Instance.GetJsonNode(_speechBubbleJson, BubbleVariationDouble));
//         }
//
//
//         /// <param name="j"></param>
//         /// <param name="preDownload">인게임 진입전 미리 다운로드 필요</param>
//         private void CollectGroupImageInfo(JsonData j, bool preDownload = false)
//         {
//             if (j == null)
//                 return;
//
//
//             // 루프 돌면서 중복이 되지않게 url 과 key를 수집한다. 
//             // bubbleDictionary에 url - key 조합으로 저장한다. 
//             // bubbleSetJSON 이 아니라 .. 
//             for (int i = 0; i < j.Count; i++)
//             {
//                 // 말풍선 스프라이트 
//                 var currentID = GetNodeValue(j[i], GameConst.COL_BUBBLE_SPRITE_ID);
//                 string currentURL;
//                 string currentKey;
//                 if (currentID != "-1")
//                 {
//                     currentURL = GetNodeValue(j[i], GameConst.COL_BUBBLE_SPRITE_URL);
//                     currentKey = GetNodeValue(j[i], GameConst.COL_BUBBLE_SPRITE_KEY);
//                     AddBubbleDictionary(currentID, currentURL, currentKey, preDownload);
//                 }
//
//                 // 말풍선 외곽선 스프라이트 
//                 currentID = GetNodeValue(j[i], GameConst.COL_BUBBLE_OUTLINE_ID);
//                 if (currentID != "-1")
//                 {
//                     currentURL = GetNodeValue(j[i], GameConst.COL_BUBBLE_OUTLINE_URL);
//                     currentKey = GetNodeValue(j[i], GameConst.COL_BUBBLE_OUTLINE_KEY);
//                     AddBubbleDictionary(currentID, currentURL, currentKey, preDownload);
//                 }
//
//                 // 말꼬리 스프라이트
//                 currentID = GetNodeValue(j[i], GameConst.COL_BUBBLE_TAIL_ID);
//                 if (currentID != "-1")
//                 {
//                     currentURL = GetNodeValue(j[i], GameConst.COL_BUBBLE_TAIL_URL);
//                     currentKey = GetNodeValue(j[i], GameConst.COL_BUBBLE_TAIL_KEY);
//                     AddBubbleDictionary(currentID, currentURL, currentKey, preDownload);
//                 }
//
//                 // 말꼬리 외곽선 
//                 currentID = GetNodeValue(j[i], GameConst.COL_BUBBLE_TAIL_OUTLINE_ID);
//                 if (currentID != "-1")
//                 {
//                     currentURL = GetNodeValue(j[i], GameConst.COL_BUBBLE_TAIL_OUTLINE_URL);
//                     currentKey = GetNodeValue(j[i], GameConst.COL_BUBBLE_TAIL_OUTLINE_KEY);
//                     AddBubbleDictionary(currentID, currentURL, currentKey, preDownload);
//                 }
//
//
//                 // 반전 말꼬리 스프라이트
//                 currentID = GetNodeValue(j[i], GameConst.COL_BUBBLE_R_TAIL_ID);
//                 if (currentID != "-1")
//                 {
//                     currentURL = GetNodeValue(j[i], GameConst.COL_BUBBLE_R_TAIL_URL);
//                     currentKey = GetNodeValue(j[i], GameConst.COL_BUBBLE_R_TAIL_KEY);
//                     AddBubbleDictionary(currentID, currentURL, currentKey, preDownload);
//                 }
//
//                 // 반전 말꼬리 외곽선 
//                 currentID = GetNodeValue(j[i], GameConst.COL_BUBBLE_R_TAIL_OUTLINE_ID);
//                 if (currentID != "-1")
//                 {
//                     currentURL = GetNodeValue(j[i], GameConst.COL_BUBBLE_R_TAIL_OUTLINE_URL);
//                     currentKey = GetNodeValue(j[i], GameConst.COL_BUBBLE_R_TAIL_OUTLINE_KEY);
//                     AddBubbleDictionary(currentID, currentURL, currentKey, preDownload);
//                 }
//
//                 // 네임태그
//                 currentID = GetNodeValue(j[i], GameConst.COL_BUBBLE_TAG_ID);
//                 if (currentID != "-1")
//                 {
//                     currentURL = GetNodeValue(j[i], GameConst.COL_BUBBLE_TAG_URL);
//                     currentKey = GetNodeValue(j[i], GameConst.COL_BUBBLE_TAG_KEY);
//                     AddBubbleDictionary(currentID, currentURL, currentKey, preDownload);
//                 }
//             }
//         }
//
//
//         /// <summary>
//         /// 버블 딕셔너리에 추가 
//         /// </summary>
//         void AddBubbleDictionary(string id, string url, string key, bool preDownload = false)
//         {
//             try
//             {
//                 if (int.Parse(id) < 0 || string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
//                     return;
//             }
//             catch
//             {
//                 Debug.Log("AddBubbleDictionary int parse exception");
//                 return;
//             }
//
//             // 필요한 컬럼이 3개라서 Dictionary를 두개 가져간다.
//             if (BubbleIDDictionary.ContainsKey(id))
//                 return;
//
//             BubbleIDDictionary[id] = url;
//             BubbleURLDictionary[url] = key;
//             // 미리 다운로드 필요시 진행 => 프로필 아이템에서 사용하는 경우 true
//             // bubbleMount 생성용도
//             if (!preDownload || _bubbleIDDictionaryForLobby.ContainsKey(id)) return;
//             
//             _bubbleIDDictionaryForLobby[id] = url;
//             _bubbleURLDictionaryForLobby[url] = key;
//         }
//
//
//         /// <summary>
//         /// 말풍선 그룹 정보 가져오기 
//         /// </summary>
//         /// <returns></returns>
//         public JsonData GetBubbleGroupJson(string template, string variation)
//         {
//             switch (template)
//             {
//                 case BubbleTalk:
//                 case GameConst.TEMPLATE_PHONE_SELF:
//                     return SystemManager.Instance.GetJsonNode(_talkBubbleJson, variation);
//                 case BubbleFeeling:
//                 case GameConst.TEMPLATE_PHONE_PARTNER:
//                     return SystemManager.Instance.GetJsonNode(_feelingBubbleJson, variation);
//                 case BubbleWhisper:
//                     return SystemManager.Instance.GetJsonNode(_whisperBubbleJson, variation);
//                 case BubbleYell:
//                     return SystemManager.Instance.GetJsonNode(_yellBubbleJson, variation);
//                 case BubbleMonologue:
//                     return SystemManager.Instance.GetJsonNode(_monologueBubbleJson, variation);
//
//                 case BubbleSpeech:
//                     return SystemManager.Instance.GetJsonNode(_speechBubbleJson, variation);
//
//                 default:
//                     return null;
//             }
//         }
//
//         #endregion
//
//
//         /// <summary>
//         /// Node 안의 value 값을 string으로 받기! 없으면 empty.
//         /// </summary>
//         private string GetNodeValue(JsonData j, string col)
//         {
//             if (j == null)
//                 return string.Empty;
//
//             if (!j.ContainsKey(col))
//                 return string.Empty;
//
//             if (j[col] == null)
//                 return string.Empty;
//
//             return j[col].ToString();
//         }
//
//         /// <summary>
//         /// 선택한 에피소드 정보 저장하기.
//         /// </summary>
//         public void SetCurrentEpisodeJson(EpisodeData data)
//         {
//             currentEpisodeData = data;
//             currentEpisodeID = currentEpisodeData.episodeID;
//         }
//
//
//         public string GetCurrentEpisodeLobbyTitle()
//         {
//             if (currentEpisodeData != null && currentEpisodeData.isValidData)
//             {
//                 return currentEpisodeData.storyLobbyTitle;
//             }
//             else
//             {
//                 return string.Empty;
//             }
//         }
//
//         #region 사이드(스페셜) 에피소드 해금 관련 로직
//
//         // * 2024.2 킬마셀부터 들어간 신규 시스템
//         // * 게임 유저들 사이에서는 크리티컬 에피소드, 컨텐츠 등으로 불립니다.
//
//         /// <summary>
//         /// 신규 사이드 에피소드 해금 체크 
//         /// </summary>
//         public void CheckSideEpisodeUnlock()
//         {
//             foreach (SideUnlockData unlockData in _listSideUnlock)
//             {
//                 unlockData.UnlockSideEpisode();
//             }
//         } // ? CheckSideEpisodeUnlock END
//
//         #endregion
//
//
//         #region 의상 정보 컨트롤
//
//         public string GetModelName(string speaker, string dressName)
//         {
//             if (_dictProjectSpeakerDress == null || _listProjectDress == null)
//             {
//                 Debug.LogError("의상 정보 없음 GetTargetDressModelNameByDressName");
//                 return null;
//             }
//
//             if (!_dictProjectSpeakerDress.ContainsKey(speaker))
//             {
//                 Debug.LogError($"{speaker} 캐릭터 정보 없음 GetTargetDressModelNameByDressName");
//                 return null;
//             }
//
//             foreach (DressData dress in _dictProjectSpeakerDress[speaker])
//             {
//                 if (dress.dress_name == dressName)
//                     return dress.model_name;
//             }
//
//             Debug.LogError($"{speaker} 캐릭터의 {dressName} 의상 정보 없음 GetTargetDressModelNameByDressName");
//
//             return null;
//         }
//
//         /// <summary>
//         /// 해당 대상코드 노드를 찾습니다. (의상정보)
//         /// </summary>
//         public DressData GetDress(string speaker, string dressName)
//         {
//             if (_dictProjectSpeakerDress == null || _listProjectDress == null)
//             {
//                 Debug.LogError("의상 정보 없음 GetTargetDressNodeByDressName");
//                 return null;
//             }
//
//             if (!_dictProjectSpeakerDress.ContainsKey(speaker))
//             {
//                 Debug.LogError($"{speaker} 캐릭터 정보 없음 GetTargetDressNodeByDressName");
//                 return null;
//             }
//
//             foreach (DressData dress in _dictProjectSpeakerDress[speaker])
//             {
//                 if (dress.dress_name == dressName)
//                     return dress;
//             }
//
//             Debug.LogError($"{speaker} 캐릭터의 {dressName} 의상 정보 없음 GetTargetDressNodeByDressName");
//             return null;
//         }
//
//         #endregion
//
//         #region 말풍선 로컬 정보
//
//         /// <summary>
//         /// 말풍선 세트 정보 조회
//         /// </summary>
//         void LoadBubbleSetLocalInfo()
//         {
//             currentBubbleSetVersion = 0;
//             var bubbleDetail = ES3.KeyExists(KeyBubbleDetailPrefix)
//                 ? ES3.Load<string>(KeyBubbleDetailPrefix)
//                 : string.Empty;
//
//             // 이 정보는 작품 상세정보를 요청할때 함께 전달합니다.
//             currentBubbleSetVersion = ES3.KeyExists(KeyBubbleVerPrefix) ? ES3.Load<int>(KeyBubbleVerPrefix) : 0;
//
//             // 데이터 없으면 version 기본값 0으로처리 
//             if (string.IsNullOrEmpty(bubbleDetail))
//             {
//                 currentBubbleSetVersion = 0;
//             }
//
//             try
//             {
//                 _currentBubbleSetJson = JsonMapper.ToObject(bubbleDetail);
//             }
//             catch (Exception e)
//             {
//                 Debug.Log(e.StackTrace);
//                 currentBubbleSetVersion = 0; // 읽다가 오류나도 0. 
//             }
//         }
//         
//         #endregion
//
//         #region 작품 상세정보
//
//         /// <summary>
//         /// 이동 컬럼(#)을 통해 다음 에피소드를 찾아가는 경우 사용 
//         /// </summary>
//         public static EpisodeData GetNextFollowingEpisodeData(int targetID)
//         {
//             foreach (var episode in Instance.listCurrentProjectEpisodes)
//             {
//                 if (episode.episodeID == targetID)
//                 {
//                     return episode;
//                 }
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 정규 에피소드 찾기 (ID로!)
//         /// </summary>
//         private EpisodeData GetRegularEpisodeByID(int episodeID)
//         {
//             foreach (var episode in Instance.listCurrentProjectEpisodes)
//             {
//                 if (episode.episodeID == episodeID)
//                 {
//                     return episode;
//                 }
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 정규 에피소드의 다음 순서 에피소드 찾기 
//         /// </summary>
//         public EpisodeData GetNextRegularEpisodeData(EpisodeData currentEpisode)
//         {
//             int nextEpisodeNumber = currentEpisode.episodeNumber + 1;
//
//             // 현재 에피소드가 엔딩이나 스페셜이면 null 리턴한다. 
//             if (currentEpisode.episodeType == EpisodeType.Ending || currentEpisode.episodeType == EpisodeType.Side)
//             {
//                 return null;
//             }
//
//             // 다음 순번을 찾는다. 
//             foreach (var episode in Instance._regularEpisodeList)
//             {
//                 if (episode.episodeNumber == nextEpisodeNumber)
//                     return episode;
//             }
//
//             // 못찾았으면 null
//             return null;
//         }
//
//         public static EpisodeData GetFirstRegularEpisodeData()
//         {
//             for (int i = 0; i < Instance._regularEpisodeList.Count; i++)
//             {
//                 if (Instance._regularEpisodeList[i].episodeType == EpisodeType.Ending ||
//                     Instance._regularEpisodeList[i].episodeType == EpisodeType.Side)
//                     continue;
//
//                 if (Instance._regularEpisodeList[i].episodeNO == "1")
//                 {
//                     return Instance._regularEpisodeList[i];
//                 }
//             }
//
//             return null;
//         }
//
//         #endregion
//
//         #region 기타 등등
//
//         public string GetCurrentEpisodeFlowPrefix()
//         {
//             if (currentEpisodeData != null && currentEpisodeData.isValidData)
//             {
//                 return currentEpisodeData.flowPrefix;
//             }
//             else
//             {
//                 return string.Empty;
//             }
//         }
//
//         /// <summary>
//         /// 게임플레이에서 누적된 스토리 획득 히스토리 노드 
//         /// </summary>
//         private JsonData GetRawStoryAbility()
//         {
//             if (!_projectDetail.ContainsKey(CommonConst.NODE_RAW_STORY_ABILITY))
//             {
//                 return null;
//             }
//
//             return _projectDetail[CommonConst.NODE_RAW_STORY_ABILITY];
//         }
//
//         /// <summary>
//         /// 스토리 누적 능력치 갱신 
//         /// </summary>
//         public void UpdateRawStoryAbility(JsonData newData)
//         {
//             _projectDetail[CommonConst.NODE_RAW_STORY_ABILITY] = newData;
//         }
//
//
//         /// <summary>
//         /// 대상 캐릭터의 능력 수치 가져오기
//         /// </summary>
//         public int GetSpeakerAbilityValue(string speaker, string ability)
//         {
//             if (!_dictStoryAbility.ContainsKey(speaker) || _dictStoryAbility[speaker] == null)
//             {
//                 Debug.LogError($"{speaker} / {ability} has no ability date");
//                 return 0;
//             }
//
//             for (int i = 0; i < _dictStoryAbility[speaker].Count; i++)
//             {
//                 if (_dictStoryAbility[speaker][i].originAbilityName == ability)
//                 {
//                     return _dictStoryAbility[speaker][i].currentValue;
//                 }
//             }
//
//             Debug.LogError($"{speaker} / {ability} 캐릭터는 있는데 능력치 이름에 해당하는 정보 없음");
//             return 0;
//         }
//
//         /// <summary>
//         /// 에피소드, 씬에서 이미 획득한 능력치 정보가 있는지 체크 
//         /// </summary>
//         public bool CheckSceneAbilityHistory(
//             int episodeID, int sceneID, string speaker, string abilityName, int value)
//         {
//             if (GetRawStoryAbility() == null)
//                 return false;
//
//             for (int i = 0; i < GetRawStoryAbility().Count; i++)
//             {
//                 if (SystemManager.Instance.GetJsonNodeInt(GetRawStoryAbility()[i], CommonConst.COL_EPISODE_ID) ==
//                     episodeID
//                     && SystemManager.Instance.GetJsonNodeInt(GetRawStoryAbility()[i], GameConst.COL_SCENE_ID) ==
//                     sceneID
//                     && SystemManager.Instance.GetJsonNodeString(GetRawStoryAbility()[i], GameConst.COL_SPEAKER) ==
//                     speaker
//                     && SystemManager.Instance.GetJsonNodeString(GetRawStoryAbility()[i], "ability_name") ==
//                     abilityName
//                     && SystemManager.Instance.GetJsonNodeInt(GetRawStoryAbility()[i], "add_value") == value)
//                     return true; // 데이터 있음 
//             }
//
//
//             return false;
//         }
//
//
//         /// <summary>
//         /// 유저 에피소드 진행도
//         /// </summary>
//         public void SetNodeUserEpisodeProgress(JsonData json)
//         {
//             _projectDetail[CommonConst.NODE_EPISODE_PROGRESS] = json;
//         }
//
//
//         /// <summary>
//         /// 유저 에피소드 히스토리에 에피소드 단일 개체 추가 
//         /// </summary>
//         public void AddUserEpisodeHistory(int playEpisodeID)
//         {
//             _projectDetail[CommonConst.NODE_EPISODE_HISTORY].Add(playEpisodeID);
//         }
//
//
//         /// <summary>
//         /// 유저 에피소드 진행도 
//         /// </summary>
//         private JsonData GetNodeUserEpisodeProgress()
//         {
//             return _projectDetail[CommonConst.NODE_EPISODE_PROGRESS];
//         }
//
//         /// <summary>
//         /// 에피소드 진행도에 단일 에피소드 추가 
//         /// </summary>
//         public void AddUserEpisodeProgress(int playEpisodeID)
//         {
//             GetNodeUserEpisodeProgress().Add(playEpisodeID);
//         }
//
//
//         /// <summary>
//         /// 에피소드 ID 진행도에 있는지 체크. 
//         /// </summary>
//         public bool CheckEpisodeProgress(int episodeID)
//         {
//             for (int i = 0; i < GetNodeUserEpisodeProgress().Count; i++)
//             {
//                 if (GetNodeUserEpisodeProgress()[i].ToString() == episodeID.ToString())
//                 {
//                     return true;
//                 }
//             }
//
//             return false;
//         }
//
//         public void SetNodeStorySceneProgress(JsonData j)
//         {
//             _projectDetail[CommonConst.NODE_SCENE_PROGRESS] = j;
//         }
//
//         /// <summary>
//         /// 현재 진행도에 대상 씬을 클리어했는지 체크 
//         /// </summary>
//         public bool CheckSceneProgress(int sceneID)
//         {
//             if (_projectDetail == null)
//                 return false;
//
//             if (!_projectDetail.ContainsKey(CommonConst.NODE_SCENE_PROGRESS))
//                 return false;
//
//             for (int i = 0; i < _projectDetail[CommonConst.NODE_SCENE_PROGRESS].Count; i++)
//             {
//                 if (_projectDetail[CommonConst.NODE_SCENE_PROGRESS][i].ToString() == sceneID.ToString())
//                     return true;
//             }
//
//             return false;
//         }
//
//         /// <summary>
//         /// 유저의 사건ID 히스토리 유무 체크
//         /// </summary>
//         public bool CheckSceneHistory(int sceneID)
//         {
//             JsonData historyScene = _projectDetail[CommonConst.NODE_SCENE_HISTORY];
//
//             if (_projectDetail == null || historyScene == null)
//             {
//                 return false;
//             }
//
//             for (int i = 0; i < historyScene.Count; i++)
//             {
//                 if (historyScene[i].ToString() == sceneID.ToString())
//                     return true;
//             }
//
//             return false;
//         }
//         
//
//         // sceneID를 작품 scene hist 에 입력하기 
//         public void AddUserProjectSceneHist(int sceneID)
//         {
//             if (_projectDetail == null || !_projectDetail.ContainsKey(CommonConst.NODE_SCENE_HISTORY))
//                 return;
//
//             // 없으면 입력하기 
//             if (!CheckSceneHistory(sceneID))
//                 _projectDetail[CommonConst.NODE_SCENE_HISTORY].Add(sceneID);
//         }
//
//
//         /// <summary>
//         /// 작품 선택지 선택 진행도 노드 저장 
//         /// </summary>
//         public void SetNodeUserProjectSelectionProgress(JsonData json)
//         {
//             _projectDetail[CommonConst.NODE_SELECTION_PROGRESS] = json;
//         }
//         
//
//         /// <summary>
//         /// 대상 에피소드에 target_scene_id를 가진 선택지 Progress 체크 
//         /// </summary>
//         public bool CheckProjectSelectionProgressExists(int episodeID, int targetSceneID)
//         {
//             if (!_projectDetail.ContainsKey(CommonConst.NODE_SELECTION_PROGRESS))
//                 return false;
//
//             if (!_projectDetail[CommonConst.NODE_SELECTION_PROGRESS].ContainsKey(episodeID.ToString()))
//                 return false;
//
//             JsonData targetEpisode = _projectDetail[CommonConst.NODE_SELECTION_PROGRESS][episodeID.ToString()];
//
//             // 에피소드별 Progress를 체크해서 ... 비교 
//             // * 지나갔던 길은 다시 체크하지 않게 수정.
//             for (int i = 0; i < targetEpisode.Count; i++)
//             {
//                 if (SystemManager.Instance.GetJsonNodeInt(targetEpisode[i], "target_scene_id") == targetSceneID
//                     && !GameManager.Instance.CheckResumeSelectionPassed(targetEpisode[i]))
//                 {
//                     Debug.Log("## Move to __targetSceneID : " + targetSceneID);
//
//                     // 루트 정보 저장하고 return true
//                     GameManager.Instance.AddResumeSelectionRoute(targetEpisode[i]);
//                     return true;
//                 }
//             }
//             
//             return false;
//         }
//
//         public bool HasLiveIllust(string illustName)
//         {
//             return _dictProjectLiveIllust.ContainsKey(illustName);
//         }
//
//         /// <summary>
//         /// 모든 정규 에피소드 플레이 상태 업데이트 (미래과거현재)
//         /// </summary>
//         private void refreshRegularEpisodePlayState()
//         {
//             if (_regularEpisodeList == null)
//                 return;
//
//             foreach (var episode in _regularEpisodeList)
//             {
//                 episode.SetEpisodePlayState();
//             }
//         }
//
//
//         /// <summary>
//         ///DLC Master Data 리스트 생성
//         /// </summary> <summary>
//         /// 
//         /// </summary>
//         private void InitDlc()
//         {
//             if (!_projectDetail.ContainsKey("dlc"))
//             {
//                 return;
//             }
//
//             listDlc = new List<DlcMasterData>();
//             for (int i = 0; i < _projectDetail["dlc"].Count; i++)
//             {
//                 DlcMasterData masterData = new DlcMasterData(_projectDetail["dlc"][i]);
//                 listDlc.Add(masterData);
//             }
//         }
//
//         void InitAdvneture()
//         {
//             if (!_projectDetail.ContainsKey("adventure"))
//             {
//                 return;
//             }
//
//             adventureData = new AdventureData(_projectDetail["adventure"]);
//         }
//
//
//         public void RequestDlcList()
//         {
//             JsonData sendingData = new JsonData();
//             sendingData["func"] = "getPackageDLC";
//
//             NetworkManager.Instance.SendPost(OnCompleteLoadDLC, sendingData, false, false);
//         }
//
//         // *
//         void OnCompleteLoadDLC(HTTPRequest request, HTTPResponse response)
//         {
//             Debug.Log(" >>> OnCompleteLoadDLC <<< ");
//
//             if (!NetworkManager.Instance.CheckResponseValidation(request, response))
//             {
//                 Debug.LogError("CallbackUpdateEpisodeRecord");
//                 return;
//             }
//
//             if (!string.IsNullOrEmpty(NetworkManager.Instance.CheckResponseResult(response.DataAsText)))
//             {
//                 return;
//             }
//
//             JsonData dlCs = JsonMapper.ToObject(response.DataAsText);
//             if (dlCs.ContainsKey("dlc"))
//             {
//                 dlCs = dlCs["dlc"];
//             }
//
//             listDlc = new List<DlcMasterData>();
//
//
//             for (int i = 0; i < dlCs.Count; i++)
//             {
//                 DlcMasterData masterData = new DlcMasterData(dlCs[i]);
//                 listDlc.Add(masterData);
//             }
//         }
//
//         /// <summary>
//         /// SceneProgress 입력하기 
//         /// </summary>
//         public void AddSceneProgress(int sceneID)
//         {
//             if (_projectDetail == null || !_projectDetail.ContainsKey(CommonConst.NODE_SCENE_PROGRESS))
//                 return;
//
//
//             if (CheckSceneProgress(sceneID))
//                 return;
//
//             _projectDetail[CommonConst.NODE_SCENE_PROGRESS].Add(sceneID);
//         }
//
//
//         public void UpdateSceneIDRecord(int sceneID)
//         {
//             // 이미 프로그레스에 있으면 통신할 필요없다. 
//             // 스킵에서 또 호출하기 싫으니까!
//             if (CheckSceneProgress(sceneID))
//                 return;
//
//             UpdateSceneIDRecord(currentEpisodeID, sceneID);
//         }
//
//         /// <summary>
//         /// 현재의 사건 ID를 기록에 추가한다. 
//         /// 사건 History, 사건 Progress 같이 추가된다. 
//         /// </summary>
//         private void UpdateSceneIDRecord(int episodeID, int sceneID)
//         {
//             // * 여기도, 이어하기를 통해 진입한 경우 통신하지 않음
//             // * 마지막 지점에 도착하면 isResumePlay는 false로 변경한다. 
//             // 수집 엔딩 보는 중이어도 통신하지 않음
//
//             JsonData j = new JsonData
//             {
//                 [CommonConst.FUNC] = "updateUserScene",
//                 [CommonConst.COL_EPISODE_ID] = episodeID,
//                 [GameConst.COL_SCENE_ID] = sceneID
//             };
//
//             NetworkManager.Instance.SendPost(null, j, false, false);
//
//             // 통신 응답을 기다리지 않고 바로 노드에 입력해준다. 
//             AddUserProjectSceneHist(sceneID);
//             AddSceneProgress(sceneID);
//         }
//
//         /// <summary>
//         /// 에피소드 진입시 사건 Progress 클리어 
//         /// </summary>
//         public void ClearSelectedEpisodeSceneProgress(int projectID, int episodeID, Action cb)
//         {
//             Debug.Log("<color=white>ClearSelectedEpisodeSceneProgress</color>");
//
//             JsonData j = new JsonData
//             {
//                 ["func"] = "deleteUserSceneProgress",
//                 ["project_id"] = projectID,
//                 ["episode_id"] = episodeID
//             };
//
//             _onCleanUserEpisodeProgress = cb;
//             NetworkManager.Instance.SendPost(CallbackClearSelectedEpisodeSceneHistory, j);
//         }
//
//         /// <summary>
//         /// 선택지 구매 목록 갱신
//         /// </summary>
//         public void SetPurchaseSelection(JsonData data, int episodeID)
//         {
//             if (!_projectDetail.ContainsKey(CommonConst.NODE_SELECTION_PURCHASE))
//             {
//                 Debug.LogError("SetPurchaseSelection : selection purchase 정보가 없음!!");
//                 return;
//             }
//
//             if (!_projectDetail[CommonConst.NODE_SELECTION_PURCHASE].ContainsKey(episodeID.ToString()))
//             {
//                 _projectDetail[CommonConst.NODE_SELECTION_PURCHASE][episodeID.ToString()] = JsonMapper.ToObject("[]");
//             }
//
//
//             _projectDetail[CommonConst.NODE_SELECTION_PURCHASE][episodeID.ToString()].Add(data);
//         }
//
//
//         /// <summary>
//         /// 해당 에피소드의 선택한 선택지를 구매한적 있는지 체크체크
//         /// </summary>
//         public bool IsPurchaseSelection(string episodeID, int selectionGroup, int selectionNo)
//         {
//             // key값이 없으면 구매한 적이 없는 에피소드
//             if (!_projectDetail[CommonConst.NODE_SELECTION_PURCHASE].ContainsKey(episodeID))
//                 return false;
//
//             JsonData selectionPurchaseData = _projectDetail[CommonConst.NODE_SELECTION_PURCHASE][episodeID];
//
//             for (int i = 0; i < selectionPurchaseData.Count; i++)
//             {
//                 // 선택지 그룹이 같은게 아니면 넘겨넘겨
//                 if (SystemManager.Instance.GetJsonNodeInt(selectionPurchaseData[i], GameConst.COL_SELECTION_GROUP) !=
//                     selectionGroup)
//                     continue;
//
//                 // 같은 선택지 그룹 내에서 같은 번호면 구매한적이 있으니 true 반환
//                 if (SystemManager.Instance.GetJsonNodeInt(selectionPurchaseData[i], GameConst.COL_SELECTION_NO) ==
//                     selectionNo)
//                     return true;
//             }
//
//             return false;
//         }
//
//         /// <summary>
//         /// 프로젝트 말풍선 세트 정보
//         /// </summary>
//         /// <returns></returns>
//         public JsonData GetNodeBubbleSet()
//         {
//             return _projectDetail[CommonConst.NODE_BUBBLE_SET];
//         }
//
//         /// <summary>
//         /// 프로젝트 말풍선 스프라이트 정보
//         /// </summary>
//         /// <returns></returns>
//         public JsonData GetNodeBubbleSprite()
//         {
//             return _projectDetail[CommonConst.NODE_BUBBLE_SPRITE];
//         }
//
//         public List<EpisodeData> GetRegularEnding()
//         {
//             return listCurrentProjectEpisodes
//                 .Where(episode => episode.episodeType == EpisodeType.Ending && episode.dlcID <= 0)
//                 .ToList();
//         }
//
//         #endregion
//
//         #region 네트워크 통신 및 콜백
//
//         /// <summary>
//         /// 선택지 구매
//         /// </summary>
//         public void PurchaseSelection(
//             int selectionGroup, int selectionNo, int price, string scriptData, OnRequestFinishedDelegate cb)
//         {
//             JsonData sending = new JsonData
//             {
//                 [CommonConst.FUNC] = "purchaseSelection",
//                 [CommonConst.COL_EPISODE_ID] = StoryManager.Instance.currentEpisodeID,
//                 [GameConst.COL_SELECTION_GROUP] = selectionGroup,
//                 [GameConst.COL_SELECTION_NO] = selectionNo,
//                 ["script_text"] = scriptData,
//                 ["price"] = price
//             };
//
//
//             NetworkManager.Instance.SendPost(cb, sending, true);
//         }
//
//
//         /// <summary>
//         /// 프로젝트 플레이 위치 저장 콜백!
//         /// </summary>
//         /// <param name="req"></param>
//         /// <param name="res"></param>
//         public void CallbackUpdateProjectCurrent(HTTPRequest req, HTTPResponse res)
//         {
//             // 통신 실패했을 때 갱신하지 않음. 
//             if (!NetworkManager.Instance.CheckResponseValidation(req, res))
//             {
//                 Debug.LogError("CallbackUpdateProjectCurrent");
//                 return;
//             }
//
//             // 갱신
//             setRegularProjectCurrent(JsonMapper.ToObject(res.DataAsText));
//         }
//
//         /// <summary>
//         /// ClearSelectedEpisodeSceneProgress 콜백
//         /// </summary>
//         /// <param name="req"></param>
//         /// <param name="res"></param>
//         void CallbackClearSelectedEpisodeSceneHistory(HTTPRequest req, HTTPResponse res)
//         {
//             if (!NetworkManager.Instance.CheckResponseValidation(req, res))
//             {
//                 Debug.LogError("CallbackClearSelectedEpisodeSceneHistory");
//                 return;
//             }
//
//             // 대상 에피소드에 속한 scene 정보만 삭제하기 때문에.. 
//             // 리스트를 갱신해서 받아와야겠다..!!!
//             // Debug.Log(res.DataAsText);
//             JsonData result = JsonMapper.ToObject(res.DataAsText);
//             if (result.ContainsKey(CommonConst.NODE_SCENE_PROGRESS))
//             {
//                 SetNodeStorySceneProgress(result[CommonConst.NODE_SCENE_PROGRESS]);
//             }
//
//             // 다 하고, 콜백 메소드 호출한다(GameManager)
//             _onCleanUserEpisodeProgress?.Invoke();
//         }
//
//         /// <summary>
//         /// 오토메 에피소드 클리어 광고보상 요청 
//         /// </summary>
//         /// <param name="request"></param>
//         /// <param name="response"></param>
//         public void CallbackRequestOtomeEpisodeAdReward(HTTPRequest request, HTTPResponse response)
//         {
//             if (!NetworkManager.Instance.CheckResponseValidation(request, response))
//                 return;
//
//             if (!string.IsNullOrEmpty(NetworkManager.Instance.CheckResponseResult(response.DataAsText)))
//             {
//                 return;
//             }
//
//             Debug.Log("### CallbackRequestOtomeEpisodeAdReward : " + response.DataAsText);
//
//
//             JsonData result = JsonMapper.ToObject(response.DataAsText);
//
//             UserManager.Instance.SetBankInfo(result);
//
//             SystemManager.Instance.ShowResourcePopup(
//                 string.Format(SystemManager.Instance.GetLocalizedText("4003"),
//                     SystemManager.Instance.GetJsonNodeInt(result, "add_value")),
//                 SystemManager.Instance.GetJsonNodeInt(result, "add_value"));
//         } // ? CallbackRequestOtomeEpisodeAdReward
//
//
//         // * 선택지 플레이기록 업데이트 콜백
//         public void CallbackUpdateUserSelectionRecord(HTTPRequest request, HTTPResponse response)
//         {
//             if (!NetworkManager.Instance.CheckResponseValidation(request, response))
//             {
//                 return;
//             }
//
//             if (!string.IsNullOrEmpty(NetworkManager.Instance.CheckResponseResult(response.DataAsText)))
//             {
//                 return;
//             }
//
//             Debug.Log("CallbackUpdateUserSelectionRecord : " + response.DataAsText);
//
//             JsonData resultSelectionRecord = JsonMapper.ToObject(response.DataAsText);
//             if (resultSelectionRecord != null)
//             {
//                 string episodeID = SystemManager.Instance.GetJsonNodeString(resultSelectionRecord, "episode_id");
//
//                 if (!_projectDetail[CommonConst.NODE_SELECTION_PROGRESS].ContainsKey(episodeID))
//                 {
//                     _projectDetail[CommonConst.NODE_SELECTION_PROGRESS][episodeID] = JsonMapper.ToObject("[]");
//                 }
//
//                 // UpdateNodeUserProjectSelectionProgress(resultSelectionRecord);
//             }
//         }
//
//
//         /// <summary>
//         /// 2022.09.14 
//         /// 에피소드 클리어 처리 최적화 버전
//         /// </summary>
//         public void RequestCompleteEpisodeOptimized(EpisodeData nextEpisode, int lastSceneID)
//         {
//             JsonData sending = new JsonData
//             {
//                 ["episode_id"] = currentEpisodeID
//             };
//
//             // 진행할 다음화가 있다면 다음화의 episode id 와 엔딩 여부를 추가해준다.
//             if (nextEpisode is { isValidData: true })
//             {
//                 sending["next_episode_id"] = nextEpisode.episodeID; // 다음 에피소드 ID 있을때만
//
//                 // 다음화가 엔딩인 경우에 대한 처리
//                 if (nextEpisode.episodeType == EpisodeType.Ending)
//                     sending["is_next_ending"] = true;
//                 else
//                     sending["is_next_ending"] = false;
//             }
//
//             // 마지막 플레이 사건ID 처리
//             if (lastSceneID > 0)
//             {
//                 // 마지막 플레이 scene_id 추가
//                 sending["scene_id"] = lastSceneID;
//                 // 통신 응답을 기다리지 않고 바로 노드에 입력해준다. 
//                 AddUserProjectSceneHist(lastSceneID);
//                 AddSceneProgress(lastSceneID);
//
//                 // CompleteMissionByScene(lastSceneID); // 미션 해금 처리 
//                 // ShowCompleteSpecialEpisodeByScene(lastSceneID); // 스페셜 에피소드 해금 처리
//             }
//
//             sending["func"] = "clearEpisode"; // 
//             sending["ver"] = 42; // 버전 2022.01.24
//             sending["useRecord"] = UserManager.Instance.useRecord;
//
//             NetworkManager.Instance.SendPost(CallbackRequestCompleteEpisode, sending);
//         }
//
//
//         /// <summary>
//         /// 유저 에피소드 클리어 콜백
//         /// </summary>
//         /// <param name="req"></param>
//         /// <param name="res"></param>
//         private void CallbackRequestCompleteEpisode(HTTPRequest req, HTTPResponse res)
//         {
//             // 통신 실패했을 때 갱신하지 않음. 
//             if (!NetworkManager.Instance.CheckResponseValidation(req, res))
//             {
//                 Debug.LogError("CallbackRequestCompleteEpisode");
//                 return;
//             }
//
//             Debug.Log(">> CallbackRequestCompleteEpisode : " + res.DataAsText);
//
//             // ! 여기에 JSON 따로 저장합니다. 
//             JsonData resultEpisodeRecord = JsonMapper.ToObject(res.DataAsText);
//
//             int playedEpisodeID =
//                 SystemManager.Instance.GetJsonNodeInt(resultEpisodeRecord, CommonConst.COL_EPISODE_ID);
//
//
//             // 노드 저장!
//             if (playedEpisodeID > 0)
//             {
//                 AddUserEpisodeHistory(playedEpisodeID); // 히스토리 
//                 AddUserEpisodeProgress(playedEpisodeID); // 진행도 
//
//                 GameManager.Instance.currentEpisodeData.isClear = true;
//             }
//
//             setRegularProjectCurrent(resultEpisodeRecord[CommonConst.NODE_PROJECT_CURRENT]);
//
//             // * 에피소드 관련 미션과 첫클리어 보상에 대한 처리는 GameManager에서 진행한다. 2022.07.27
//         }
//
//
//         /// <summary>
//         /// 에피소드 리셋 프로그레스 콜백
//         /// </summary>
//         /// <param name="req"></param>
//         /// <param name="res"></param>
//         public void CallbackResetEpisodeProgress(HTTPRequest req, HTTPResponse res)
//         {
//             if (!NetworkManager.Instance.CheckResponseValidation(req, res))
//             {
//                 Debug.LogError("CallbackResetEpisodeProgress");
//                 return;
//             }
//
//             if (!string.IsNullOrEmpty(NetworkManager.Instance.CheckResponseResult(res.DataAsText)))
//             {
//                 return;
//             }
//
//
//             JsonData resultEpisodeReset = JsonMapper.ToObject(res.DataAsText);
//
//             SetNodeUserEpisodeProgress(resultEpisodeReset[CommonConst.NODE_EPISODE_PROGRESS]); // 에피소드 progress 
//             SetNodeStorySceneProgress(resultEpisodeReset[CommonConst.NODE_SCENE_PROGRESS]); // 씬 progress
//             setRegularProjectCurrent(resultEpisodeReset[CommonConst.NODE_PROJECT_CURRENT]); // projectCurrent
//             SetNodeUserProjectSelectionProgress(resultEpisodeReset[CommonConst.NODE_SELECTION_PROGRESS]); // 선택지 기록 
//             UpdateRawStoryAbility(resultEpisodeReset[CommonConst.NODE_RAW_STORY_ABILITY]);
//             SetStoryAbilityDictionary(resultEpisodeReset[CommonConst.NODE_USER_ABILITY], true);
//
//             RefreshProfile();
//             UserManager.Instance.SetBankInfo(resultEpisodeReset);
//             UserManager.Instance.SetPropertyData(resultEpisodeReset); // 프로퍼티 데이터 수정 
//             // 알림 팝업 후 목록화면 갱신처리 
//             SystemManager.Instance.ShowOneButtonMessagePopupLocalize("6167");
//             // * Doozy Nody StoryDetail로 돌아가기 위한 이벤트 생성 
//             // * ViewStoryDetail 에서 이 시그널을 Listener를 통해서 받는다. (Inspector)
//             // refresh 플레이 상태 
//             refreshRegularEpisodePlayState();
//
//             // 스토리매니저의 스토리 상태정보 업데이트 
//             StoryLobbyManager.Instance.InitMainStoryProcess();
//
//             // 로비 타이틀 갱신
//             ViewLobby.ActionRefreshCurrentEpisode?.Invoke();
//             Signal.Send(LobbyConst.STREAM_COMMON, "RedirectHome"); // Home 화면으로 이동.
//         }
//
//         /// <summary>
//         /// 처음으로 돌아가기 콜백 
//         /// </summary>
//         /// <param name="req"></param>
//         /// <param name="res"></param>
//         public void CallbackStartOverEpisode(HTTPRequest req, HTTPResponse res)
//         {
//             if (!NetworkManager.Instance.CheckResponseValidation(req, res))
//             {
//                 Debug.LogError("CallbackStartOverEpisode");
//                 return;
//             }
//
//             JsonData resultEpisodeReset = JsonMapper.ToObject(res.DataAsText);
//             SetNodeStorySceneProgress(resultEpisodeReset[CommonConst.NODE_SCENE_PROGRESS]); // 씬 progress
//             setRegularProjectCurrent(resultEpisodeReset[CommonConst.NODE_PROJECT_CURRENT]); // projectCurrent
//             SetNodeUserProjectSelectionProgress(resultEpisodeReset[CommonConst.NODE_SELECTION_PROGRESS]); // 선택지 기록 
//             UpdateRawStoryAbility(resultEpisodeReset[CommonConst.NODE_RAW_STORY_ABILITY]);
//             SetStoryAbilityDictionary(resultEpisodeReset[CommonConst.NODE_USER_ABILITY], true);
//
//
//             // 통신 완료 후 게임매니저 메소드 호출 
//             GameManager.Instance.RetryPlay();
//         }
//
//         #endregion
//     }
// }