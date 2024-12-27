using UnityEngine;


namespace Code.Models.RoleModel
{
    public class RoleModel
    {
        private static RoleModel _roleModel;

        public static RoleModel Instance
        {
            get
            {
                if (_roleModel == null)
                {
                    _roleModel = new RoleModel();
                }
                return _roleModel;
            }
        }
                
        private RoleTypes m_currentRole;

        public RoleTypes CurrentRole
        {
            get
            {
                return m_currentRole;
            }
            set
            {
                m_currentRole = value; 
            }
        }

        public bool Equals(RoleTypes role)
        {
            return CurrentRole == role;
        }

        public bool Contains(RoleTypes[] roles)
        {
            foreach (var role in roles)
                if (role == CurrentRole) return true;

            return false;
        }

    }
}