using Lexplosion.UI.WPF.Core.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;
using System.Windows.Media;

namespace Lexplosion.UI.WPF.Mvvm.Models.Profile
{
    public struct Post 
    {
        public string AuthorName { get; }
        public ProfileTitle AuthorTitle { get; }
        public string Text { get; set; }
        public int ViewsCount { get; }
        public DateTime CreationDate { get; }
        public int LikesCount { get; set; }
    }

    public class PostComment 
    {
        public string AuthorName { get; }
        public string Content { get; }
        public DateTime CreationDate { get; }
        public List<PostComment> Answers { get; } = new();
    }

    public class ProfileModel : ObservableObject
    {
        public ObservableCollection<Post> Posts { get; } = new();
        public ProfileInfo ProfileInfo { get; }

        public ProfileModel(ProfileInfo profileInfo)
        {
            ProfileInfo = profileInfo;
        }
    }
}
