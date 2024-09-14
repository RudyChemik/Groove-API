namespace Groove.VM.PayPal
{
    public class WebhookEventHeaders
    {
        public string TransmissionId { get; }
        public string TransmissionTime { get; }
        public string TransmissionSig { get; }
        public string CertUrl { get; }
        public string AuthAlgo { get; }
        public string CertId { get; }

        public WebhookEventHeaders(
            string transmissionId,
            string transmissionTime,
            string transmissionSig,
            string certUrl,
            string authAlgo,
            string certId)
        {
            TransmissionId = transmissionId;
            TransmissionTime = transmissionTime;
            TransmissionSig = transmissionSig;
            CertUrl = certUrl;
            AuthAlgo = authAlgo;
            CertId = certId;
        }
    }

}
