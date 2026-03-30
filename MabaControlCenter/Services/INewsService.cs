using System.Collections.ObjectModel;
using MabaControlCenter.Models;

namespace MabaControlCenter.Services;

public interface INewsService
{
    ObservableCollection<NewsItem> NewsItems { get; }
}
