using System;
using System.Collections.Generic;
using Code.Controllers.MessageBox;
using Code.Models.REST;
using Code.Models.REST.Administrative;
using Code.Models.REST.Group;
using Code.Models.REST.Users;
using Newtonsoft.Json;
using Proyecto26;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Models
{
    public class GroupModel
    {
        public event EventHandler<EventArgs> OnListChanged;

        protected virtual void ListChanged(EventArgs e)
        {
            EventHandler<EventArgs> handler = OnListChanged;
            handler?.Invoke(this, e);
        }

        public GroupModel() { }

        private Group myGroup = null;
        public Group MyGroup { get => myGroup; set => myGroup = value; }

        public RSG.IPromise<DataModelOperationResult> GetGroup()
        {
            GetGroupRequest req = new GetGroupRequest();

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new GetGroupResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        //Запросить свежую информацию о группе
        public RSG.IPromise<DataModelOperationResult> UpdateGroupItem()
        {
            var prom = GetGroup()
                .Then((res) =>
                {
                    if (res.result)
                    {                        
                        MyGroup = ((GetGroupResponse)res.ParsedResponse).MyGroup;
                        ListChanged(EventArgs.Empty);
                    }                    

                    return res;
                });

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> UpdateGroup(string groupName, string groupImg)
        {
            UpdateGroupRequest req = new UpdateGroupRequest(groupName, groupImg);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new UpdateGroupResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }

        public RSG.IPromise<DataModelOperationResult> ProccessPurchase(string productId, string purchaseToken)
        {
            ProccessPurchaseRequest req = new ProccessPurchaseRequest(productId, purchaseToken);

            var prom = RestClientEx.PostEx(req.request)
               .Then((res) => DataModelOperationResult.Wrap(res.RawResponse, new ProccessPurchaseResponse(res.RawResponse)))
               .Catch((ex) => DataModelOperationResult.Wrap(ex));

            return prom;
        }
    }
}
