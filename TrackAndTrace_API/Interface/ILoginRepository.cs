using TrackAndTrace_API.Models.RequestModel;
using TrackAndTrace_API.Models.ResponseModel;

namespace TrackAndTrace_API.Interface
{
    public interface ILoginRepository
    {
        Task<APIResponseDTO> AuthenticateUser(LoginDto? model, string email);
    }
}
