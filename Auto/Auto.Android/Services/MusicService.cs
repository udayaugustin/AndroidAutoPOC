using Android.OS;
using Android.Support.V4.Media;
using AndroidX.Media.Utils;
using static Android.Media.Browse.MediaBrowser;
using System.Collections.Generic;
using static Android.Support.V4.Media.MediaBrowserCompat;

public class MusicService
{
    public List<MediaBrowserCompat.MediaItem> GetMediaItems()
    {
        var mediaItems = new List<MediaBrowserCompat.MediaItem>();
        mediaItems.Add(CreateMediaItem("media_id_1", "Song 1", "Artist 1", "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3", "https://fastly.picsum.photos/id/531/200/200.jpg?hmac=PE_UHXALavopqDJ2V1rSz0nCsrJQX3c6rgUPXndBkwo"));
        mediaItems.Add(CreateMediaItem("media_id_2", "Song 2", "Artist 2", "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3", "https://fastly.picsum.photos/id/163/200/200.jpg?hmac=mEG0MVDQnbY2PIFVIxZKgINnXrapgb5G5S1QMtMTt98"));
        mediaItems.Add(CreateMediaItem("media_id_3", "Song 3", "Artist 3", "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3", "https://fastly.picsum.photos/id/608/200/200.jpg?hmac=-p1htX-mFieavdRDr9vUIJKyDHCXZAY5B35nhdcgIgQ"));
        mediaItems.Add(CreateMediaItem("media_id_4", "Song 4", "Artist 4", "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-4.mp3", "https://fastly.picsum.photos/id/705/200/200.jpg?hmac=2HZlwayMAMOyCllDpM-Mx3u2Xyk40VRHAzlpNLKyTC8"));

        return mediaItems;
    }


    private MediaBrowserCompat.MediaItem CreateMediaItem(string mediaId, string title, string artist, string mediaUri, string albumUri)
    {
        var description = new MediaDescriptionCompat.Builder()
            .SetMediaId(mediaId)
            .SetTitle(title)
            .SetSubtitle(artist)
            .SetIconUri(Android.Net.Uri.Parse(albumUri))
            .SetMediaUri(Android.Net.Uri.Parse(mediaUri));
        Bundle extras = new Bundle();
        extras.PutInt(
            MediaConstants.DescriptionExtrasKeyContentStyleBrowsable,
            MediaConstants.DescriptionExtrasValueContentStyleCategoryListItem);
        extras.PutInt(
            MediaConstants.DescriptionExtrasKeyContentStyleBrowsable,
            MediaConstants.DescriptionExtrasValueContentStyleListItem);
        extras.PutInt(
            MediaConstants.DescriptionExtrasKeyContentStylePlayable,
            MediaConstants.DescriptionExtrasValueContentStyleGridItem);        

        description.SetExtras(extras);
        return new MediaBrowserCompat.MediaItem(description.Build(), MediaBrowserCompat.MediaItem.FlagPlayable);
    }
}