using Proyecto26;

namespace Code.Models.REST.Notifications
{
    public class RegisterDeviceRequest : FQRequestInfoCreator
    {
        public override string ActionName => "RegisterDevice";
        public override object PostData => _NotifiedDevice;

        NotifiedDevice _NotifiedDevice;
        public RegisterDeviceRequest() { }

        public RegisterDeviceRequest(NotifiedDevice notifiedDevice)
        {
            _NotifiedDevice = notifiedDevice;
            FillRequest();
        }
    }

    /// <summary>
    /// Ответ
    /// </summary>
    public class RegisterDeviceResponse : FQResponse
    {
        public bool Registered
        {
            get
            {
                return this.ri.DeserializeResponseData<bool>();
            }
        }

        public RegisterDeviceResponse(ResponseHelper response) : base(response)
        { }
    }
}
