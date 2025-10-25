using System.Threading.Tasks;

namespace OnlineRadioStation.Services 
{
    public interface ICommand
    {
        Task ExecuteAsync();
    }
}