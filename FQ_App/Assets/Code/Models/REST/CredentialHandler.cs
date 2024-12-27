using Code.Models.REST.Users;
using Code.Models.RoleModel;

namespace Code.Models.REST
{
    class CredentialHandler
    {
        static CredentialHandler _instance;

        public static CredentialHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new CredentialHandler();

                return _instance;
            }
        }

        //Зарезервирован. В текущей версии выполняется локальная проверка на каждом устройстве.
        public bool isFirstLogin = false;

        public UserCredentials Credentials { get => credentials; set => credentials = value; }

        public User CurrentUser { get => user; set => user = value; }

        public CredentialHandler()
        {
            credentials = new UserCredentials();
            user = new User();            
        }

        private UserCredentials credentials;

        private User user;
    }
}
