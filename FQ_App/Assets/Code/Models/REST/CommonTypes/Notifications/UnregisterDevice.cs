using Proyecto26;

namespace Code.Models.REST.Notifications
{
    public class UnregisterDeviceRequest : FQRequestInfoCreator
    {
        public override string ActionName => "RegisterDevice";
        public override object PostData => _NotifiedDevice;

        NotifiedDevice _NotifiedDevice;
        public UnregisterDeviceRequest() { }

        public UnregisterDeviceRequest(NotifiedDevice notifiedDevice)
        {
            _NotifiedDevice = notifiedDevice;
            FillRequest();
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class UnregisterDeviceResponse : FQResponse
    {
        public bool Registered
        {
            get
            {
                return this.ri.DeserializeResponseData<bool>();
            }
        }

        public UnregisterDeviceResponse(ResponseHelper response) : base(response)
        { }
    }
}
