using System.IO;
using System.Threading.Tasks;

namespace AIApp
{
    public interface IPhotoDetector
    {
        Task<FriesOrNotFriesTag> DetectAsync(Stream photo);
    }

    public enum FriesOrNotFriesTag
    {
        None,
        Fries,
        NotFries,
    }
}
