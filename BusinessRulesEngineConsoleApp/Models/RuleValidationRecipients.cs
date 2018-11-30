using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessRulesEngineConsoleApp.Models
{
    [Table("rules.RuleValidationRecipients")]
    public class RuleValidationRecipients
    {
        [Key]
        [Column(Order = 0)]
        public string EmailAddress { get; set; }
    }
}
