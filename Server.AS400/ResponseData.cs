using System;
using Newtonsoft.Json;

namespace Server.AS400
{
    public class ResponseDataJson
    {
        public string Message { get; set; }
        public string MessageRpta { get; set; }
        public string IdUnico { get; set; }
        public string SystemTraceAudit { get; set; }
        public string ResponseCode { get; set; }
        public string Datocuota { get; set; }
        public string Puntos { get; set; }
        public string FechaEOS { get; set; }
        public string FechaSOS { get; set; }
        public string FechaEA4 { get; set; }
        public string FechaSA4 { get; set; }
        public string AdditionalDataResponse { get; set; }
        public string GlosaPSI { get; set; }
        public string CAVVVResultCode { get; set; }
        public string AuthIdResponse { get; set; }
        public string PMCPNetData { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorType { get; set; }
        public string TxnID { get; set; }

        /// <summary>
        /// Genera datos aleatorios para la instancia de la respuesta.
        /// </summary>
        public static ResponseDataJson GenerateRandomData()
        {
            var random = new Random();
            var traceAudit = random.Next(100000, 999999).ToString();
            var responseCode = random.Next(0, 100).ToString("00");
            var authId = random.Next(100000, 999999).ToString();
            var txnId = random.Next(1000000, 9999999).ToString();
            var idUnico = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}{random.Next(1000, 9999)}";
            var dateNow = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");

            return new ResponseDataJson
            {
                Message = "0110",
                MessageRpta = responseCode == "00" ? "Aprobado" : "Rechazado",
                IdUnico = idUnico,
                SystemTraceAudit = traceAudit,
                ResponseCode = responseCode,
                Datocuota = "0000",
                Puntos = "",
                FechaEOS = dateNow,
                FechaSOS = dateNow,
                FechaEA4 = dateNow,
                FechaSA4 = DateTime.Now.AddSeconds(random.Next(1, 5)).ToString("dd/MM/yyyy hh:mm:ss"),
                AdditionalDataResponse = "Datos adicionales de prueba",
                GlosaPSI = "",
                CAVVVResultCode = "",
                AuthIdResponse = authId,
                PMCPNetData = "",
                ErrorMessage = responseCode == "00" ? null : "Transacción rechazada",
                ErrorType = responseCode == "00" ? null : "Negocio",
                TxnID = txnId
            };
        }

        /// <summary>
        /// Serializa la instancia a una cadena JSON.
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
