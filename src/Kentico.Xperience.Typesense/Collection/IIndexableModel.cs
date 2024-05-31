using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.Xperience.Typesense.Collection;
public interface ICollectionableModel
{
    public Guid ItemGuid { get; set; }
    public string ContentTypeName { get; set; }
}
