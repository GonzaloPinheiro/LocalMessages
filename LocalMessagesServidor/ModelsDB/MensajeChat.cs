using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalMessagesServidor.ModelsDB
{
    /// <summary>
    /// Representa un mensaje enviado en el chat, con opción de ser público o privado.
    /// </summary>
    [Table("MensajesChat")]
    public class MensajeChat
    {
        /// <summary>
        /// Identificador único del mensaje (clave primaria).
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Nombre del usuario que envió el mensaje.
        /// </summary>
        public string Remitente { get; set; }

        /// <summary>
        /// Nombre del receptor (si es privado). Si es null, el mensaje es público.
        /// </summary>
        public string Receptor { get; set; }

        /// <summary>
        /// Contenido textual del mensaje.
        /// </summary>
        public string Contenido { get; set; }

        /// <summary>
        /// Fecha y hora en que se envió el mensaje.
        /// </summary>
        public DateTime FechaEnvio { get; set; }
    }
}
