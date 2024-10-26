using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using WPF_HW.Models;

namespace WPF_HW.ViewModels
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private const string ApiUrl = "http://localhost:5000/api/users";
        private HttpClient _httpClient;

        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        public User NewUser { get; set; } = new User();

        public ICommand AddUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public UserViewModel()
        {
            _httpClient = new HttpClient();
            AddUserCommand = new RelayCommand(async _ => await AddUser());
            DeleteUserCommand = new RelayCommand(async id => await DeleteUser((int)id));
            LoadUsers();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        
        public async void LoadUsers()
        {
            var response = await _httpClient.GetStringAsync(ApiUrl);
            var users = JsonConvert.DeserializeObject<ObservableCollection<User>>(response);
            Users.Clear();
            foreach (var user in users)
            {
                Users.Add(user);
            }
        }

        public async Task AddUser()
        {
            var json = JsonConvert.SerializeObject(NewUser);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var addedUser = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
                Users.Add(addedUser);
                NewUser = new User();
                OnPropertyChanged(nameof(NewUser));
            }
        }

       
        public async Task DeleteUser(int id)
        {
            var response = await _httpClient.DeleteAsync($"{ApiUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var userToRemove = Users.FirstOrDefault(u => u.Id == id);
                if (userToRemove != null)
                    Users.Remove(userToRemove);
            }
        }
    }
}
