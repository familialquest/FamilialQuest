using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using UnityEngine.Networking;
using System.Text;
using Code.Models.REST.CommonType.Tasks;
using Code.Models.REST;
using System;
using static Assets.Code.Models.REST.CommonTypes.FQServiceException;
using Assets.Code.Models.REST.CommonTypes;
using Code.Controllers.MessageBox;
using UnityEngine.SceneManagement;
using Code.Models.REST.Users;

namespace Code.Models
{
    public class DataModel// : ScriptableObject
    {
        private static DataModel instance;
        public static DataModel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataModel(); //ScriptableObject.CreateInstance<DataModel>();
                }
                return instance;
            }
        }

        private static readonly string DefaultServerUri = "[TODO]";
        private Uri m_serverUri = new Uri(DefaultServerUri);
        public Uri ServerUri
        {
            get => new Uri(PlayerPrefs.GetString("ServerUri", DefaultServerUri));
        }

        private readonly string DefaultServerAddress = "[TODO]";
        private string m_serverAddress = "";
        public string ServerAddress
        {
            get => ServerUri.Host;
            //get
            //{
            //    if (string.IsNullOrEmpty(m_serverAddress))
            //        m_serverAddress =  PlayerPrefs.GetString("ServerAddress", DefaultServerAddress);

            //    return m_serverAddress;
            //}
        }
        private readonly string DefaultServerPort = "443";
        private string m_serverPort = "";
        public string ServerPort
        {
            get => string.IsNullOrEmpty(ServerUri.Port.ToString()) ? DefaultServerPort : ServerUri.Port.ToString();
            //get
            //{
            //    if (string.IsNullOrEmpty(m_serverPort))
            //        m_serverPort = PlayerPrefs.GetString("ServerPort", DefaultServerPort); ;

            //    return m_serverPort;
            //}
        }
        private readonly string DefaultServerProtocol = "https";
        private string m_serverProtocol = "";
        public string ServerProtocol
        {
            get => string.IsNullOrEmpty(ServerUri.Scheme) ? DefaultServerProtocol : ServerUri.Scheme;
            //get         
            //{
            //    if (string.IsNullOrEmpty(m_serverProtocol))
            //        m_serverProtocol = PlayerPrefs.GetString("ServerUri", DefaultServerProtocol);   

            //    return m_serverProtocol;
            //}
        }

        public DataModel()
        {
            Auth = new AuthModel();
            Tasks = new TaskModel();
            Rewards = new RewardModel();
            Credentials = new CredentialsModel();
            GroupInfo = new GroupModel();
            HistoryEvents = new HistoryEventModel();
            Notifications = new NotificationModel();
        }

        public AuthModel Auth;

        public TaskModel Tasks;

        public RewardModel Rewards;

        public CredentialsModel Credentials;

        public GroupModel GroupInfo;

        public HistoryEventModel HistoryEvents;

        public NotificationModel Notifications;
    }

    public class DataModelOperationResult
    {
        public ResponseHelper RawResponse;
        public FQResponse ParsedResponse;

        public bool result = false;
        public long code = 0L;
        public string status = "";
        public string description = "";

        public DataModelOperationResult()
        {

        }

        public DataModelOperationResult(FQServiceExceptionType fqExType, bool showExMessage = true)
        {
            result = false;
            code = 0L;
            status = "";
            description = "";

            if (showExMessage)
            {
                if (!FQServiceException.msgBoxOpened)
                {
                    FQServiceException.ShowExceptionMessage(fqExType)
                        .Then((dialogRes) =>
                        {
                            FQServiceException.msgBoxOpened = false;
                        })
                        .Catch((ex) =>
                        {
                            FQServiceException.msgBoxOpened = false;
                        });
                }
            }
        }

        public DataModelOperationResult(ResponseHelper in_response, FQResponse in_parsed, bool GeneralParsing)
        {
            RawResponse = in_response;
            ParsedResponse = in_parsed;
            ParseResult(GeneralParsing);
        }


        public void ParseResult(bool GeneralParsing = false)
        {
            result = ParsedResponse.ri.Successfuly;

            FQServiceExceptionType _exType = FQServiceExceptionType.DefaultError;

            if (!result && !GeneralParsing)
            {
                if (string.IsNullOrEmpty(CredentialHandler.Instance.Credentials.tokenB64))
                {
                    if (!String.IsNullOrEmpty(ParsedResponse.ri.ResponseData) &&
                        Enum.TryParse(ParsedResponse.ri.ResponseData, out _exType) &&
                        _exType == FQServiceExceptionType.UnsupportedClientVersion)
                    {
                        if (!FQServiceException.msgBoxOpened)
                        {
                            FQServiceException.ShowExceptionMessage(FQServiceExceptionType.UnsupportedClientVersion)
                                .Then((dialogRes) =>
                                {
                                    //Чтобы после перезахода не проскользнул старый мусор
                                    DataModel.Instance.Credentials.Users = null;
                                    DataModel.Instance.GroupInfo.MyGroup = null;
                                    DataModel.Instance.Rewards.Rewards = null;
                                    DataModel.Instance.Tasks.Tasks = null;
                                    DataModel.Instance.HistoryEvents.HistoryEvents = null;

                                    CredentialHandler.Instance.Credentials = new UserCredentials();
                                    CredentialHandler.Instance.CurrentUser = new User();

                                    FQServiceException.msgBoxOpened = false;

                                    PlayerPrefs.SetString("AuthToken", string.Empty);

                                    Controllers.NotificationController.Instance.Uninit();

                                    SceneManager.LoadScene("StartPages", LoadSceneMode.Single);
                                })
                                .Catch((ex) =>
                                {
                                    FQServiceException.msgBoxOpened = false;
                                });                            
                        }
                    }
                    else
                    {
                        if (!FQServiceException.msgBoxOpened)
                        {
                            FQServiceExceptionType exType = FQServiceExceptionType.DefaultError;

                            //Показ различного текста ошибки при логине/проверка токена
                            if (SceneManager.GetActiveScene().name == "StartPages")
                            {
                                if (_exType == FQServiceExceptionType.WrongConfirmCode)
                                {
                                    exType = FQServiceExceptionType.WrongConfirmCode;
                                }

                                if (_exType == FQServiceExceptionType.IncorrectLoginFormat)
                                {
                                    exType = FQServiceExceptionType.IncorrectLoginFormat;
                                }

                                if (_exType == FQServiceExceptionType.AuthError)
                                {
                                    exType = FQServiceExceptionType.AuthError;
                                }

                                FQServiceException.ShowExceptionMessage(exType)
                                    .Then((dialogRes) =>
                                    {
                                        FQServiceException.msgBoxOpened = false;
                                    })
                                    .Catch((ex) =>
                                    {
                                        FQServiceException.msgBoxOpened = false;
                                    });
                            }
                            else
                            {
                                FQServiceException.ShowExceptionMessage(exType)
                                    .Then((dialogRes) =>
                                    {
                                        //Чтобы после перезахода не проскользнул старый мусор
                                        DataModel.Instance.Credentials.Users = null;
                                        DataModel.Instance.GroupInfo.MyGroup = null;
                                        DataModel.Instance.Rewards.Rewards = null;
                                        DataModel.Instance.Tasks.Tasks = null;
                                        DataModel.Instance.HistoryEvents.HistoryEvents = null;

                                        CredentialHandler.Instance.Credentials = new UserCredentials();
                                        CredentialHandler.Instance.CurrentUser = new User();

                                        FQServiceException.msgBoxOpened = false;

                                        PlayerPrefs.SetString("AuthToken", string.Empty);

                                        Controllers.NotificationController.Instance.Uninit();

                                        SceneManager.LoadScene("StartPages", LoadSceneMode.Single);
                                    })
                                    .Catch((ex) =>
                                    {
                                        FQServiceException.msgBoxOpened = false;
                                    });
                            }
                        }
                    }
                }
                else
                {
                    if (!FQServiceException.msgBoxOpened)
                    {
                        FQServiceException.msgBoxOpened = true;
                        FQServiceException.ShowExceptionMessage(ParsedResponse.ri.ResponseData)
                            .Then((dialogRes) =>
                            {
                                FQServiceException.msgBoxOpened = false;

                            })
                            .Catch((ex) =>
                            {
                                FQServiceException.msgBoxOpened = false;
                            });
                    }
                }
            }
        }

        public static DataModelOperationResult Wrap(ResponseHelper in_response, FQResponse in_parsed, bool GeneralParsing = false)
        {
            CredentialHandler.Instance.Credentials.userId = in_parsed.ri.UserId;
            CredentialHandler.Instance.Credentials.tokenB64 = in_parsed.ri.ActualToken;

            try
            {
                PlayerPrefs.SetString("AuthToken", CredentialHandler.Instance.Credentials.tokenB64);
            }
            catch
            {
               
            }

            return new DataModelOperationResult(in_response, in_parsed, GeneralParsing);
        }
        public static DataModelOperationResult Wrap(Exception ex)
        {
            var requestError = (Proyecto26.RequestException)ex;

            if (SceneManager.GetActiveScene().name == "StartPages")
            {
                string notNetworkError = "Cannot resolve destination host"; //Других полей для идентификации не обнаружено

                if (requestError.IsNetworkError &&
                    ex.Message == notNetworkError) //второе условие необходимо, т.к. на железе отрабатывает иначе
                {
                    return new DataModelOperationResult(FQServiceExceptionType.NetworkError);
                }
                else
                {
                    if ((requestError.IsHttpError && requestError.StatusCode >= 500 && requestError.StatusCode <= 526) ||
                        ex.Message != notNetworkError) //второе условие необходимо, т.к. на железе отрабатывает иначе
                    {
                        Proyecto26.RestClient.Get("https://raw.githubusercontent.com/familialquest/publicData/main/TechMessage.txt")
                            .Then((res) =>
                            {
                                string techMessage = string.Empty;

                                try
                                {
                                    techMessage = res.Text;

                                    if (!string.IsNullOrEmpty(techMessage))
                                    {
                                        techMessage = techMessage.Replace("NN", "\n\n");

                                        if (!FQServiceException.msgBoxOpened)
                                        {
                                            FQServiceException.ShowExceptionMessage(FQServiceException.FQServiceExceptionType.DefaultError, techMessage)
                                                .Then((dialogRes) =>
                                                {
                                                    FQServiceException.msgBoxOpened = false;

                                                })
                                                .Catch((dialogEx) =>
                                                {
                                                    FQServiceException.msgBoxOpened = false;
                                                });
                                        }
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                                catch
                                {
                                    FQServiceException.ShowExceptionMessage(FQServiceException.FQServiceExceptionType.DefaultError)
                                        .Then((dialogRes) =>
                                        {
                                            FQServiceException.msgBoxOpened = false;

                                        })
                                        .Catch((dialogEx) =>
                                        {
                                            FQServiceException.msgBoxOpened = false;
                                        });
                                }
                            })
                            .Catch((exGet) =>
                            {
                                FQServiceException.ShowExceptionMessage(FQServiceException.FQServiceExceptionType.DefaultError)
                                    .Then((dialogRes) =>
                                    {
                                        FQServiceException.msgBoxOpened = false;

                                    })
                                    .Catch((dialogEx) =>
                                    {
                                        FQServiceException.msgBoxOpened = false;
                                    });
                            });

                        return new DataModelOperationResult(FQServiceExceptionType.DefaultError, false);
                    }
                    else
                    {
                        return new DataModelOperationResult(FQServiceExceptionType.DefaultError);
                    }
                }
            }
            else
            {
                FQServiceException.ShowExceptionMessage(FQServiceExceptionType.NetworkError)
                    .Then((dialogRes) =>
                    {
                        //Чтобы после перезахода не проскользнул старый мусор
                        DataModel.Instance.Credentials.Users = null;
                        DataModel.Instance.GroupInfo.MyGroup = null;
                        DataModel.Instance.Rewards.Rewards = null;
                        DataModel.Instance.Tasks.Tasks = null;
                        DataModel.Instance.HistoryEvents.HistoryEvents = null;

                        CredentialHandler.Instance.Credentials = new UserCredentials();
                        CredentialHandler.Instance.CurrentUser = new User();

                        FQServiceException.msgBoxOpened = false;

                        PlayerPrefs.SetString("AuthToken", string.Empty);

                        Controllers.NotificationController.Instance.Uninit();

                        SceneManager.LoadScene("StartPages", LoadSceneMode.Single);
                    })
                    .Catch((dialogEx) =>
                    {
                        FQServiceException.msgBoxOpened = false;
                    });

                return new DataModelOperationResult(FQServiceExceptionType.NetworkError, false);
            }
        }
    }

}