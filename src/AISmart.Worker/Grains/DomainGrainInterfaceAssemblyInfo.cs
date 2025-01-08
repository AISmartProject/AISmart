using AISmart.Domain.Grains.Authors;
using Orleans;

[assembly: GenerateCodeForDeclaringAssembly(typeof(INameValidator))]
//add more grain interfaces below this line

namespace AISmart.Grains;