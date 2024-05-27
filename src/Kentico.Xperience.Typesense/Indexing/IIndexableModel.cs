using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.Xperience.TypeSense.Indexing;
public interface IIndexableModel
{
    public Guid ItemGuid { get; set; }
    public string ContentTypeName { get; set; }
}
