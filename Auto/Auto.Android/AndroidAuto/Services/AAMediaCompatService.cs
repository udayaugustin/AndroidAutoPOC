using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Media.Session;
using Android.Support.V4.Media;
using AndroidX.Media;
using System.Collections.Generic;
using static Android.Support.V4.Media.MediaBrowserCompat;
using Android.Media;
using AndroidX.Media.Utils;
using Android.Media.Metrics;
using Android.Icu.Util;
using MediaManager;
using System.Threading.Tasks;
using System.Linq;

namespace Auto.Droid.AndroidAuto.Services
{
    [Service(Exported = true)]
    [IntentFilter(new[] { "android.media.browse.MediaBrowserService" })]
    public class AAMediaCompatService : MediaBrowserServiceCompat
    {
        private static MediaSessionCompat mediaSession;
        private MediaSessionCompat.Callback mediaSessionCallback;
        private static List<MediaItem> mediaItems;

        public override void OnCreate()
        {
            base.OnCreate();

            mediaSession = new MediaSessionCompat(this, "MediaPlaybackService");
            mediaSession.SetFlags(1);

            SessionToken = mediaSession.SessionToken;

            mediaSessionCallback = new MediaSessionCallback();
            mediaSession.SetCallback(mediaSessionCallback);

            
            mediaSession.Active = true;
        }

        public override BrowserRoot OnGetRoot(string clientPackageName, int clientUid, Bundle rootHints)
        {
            Bundle extras = new Bundle();
            extras.PutInt(
            MediaConstants.DescriptionExtrasKeyContentStyleBrowsable,
            MediaConstants.DescriptionExtrasValueContentStyleGridItem);
            
            extras.PutInt(
                MediaConstants.DescriptionExtrasKeyContentStylePlayable,
                MediaConstants.DescriptionExtrasValueContentStyleListItem);
            
            // Determine the root node for browsing
            return new BrowserRoot("root_id", extras);
        }

        public override void OnLoadChildren(string parentId, Result result)
        {            
            var musicService = new MusicService();
            mediaItems = musicService.GetMediaItems();
            var mediaItemList = new JavaList<MediaBrowserCompat.MediaItem>(musicService.GetMediaItems());
            result.SendResult(mediaItemList);
        }

        private MediaItem CreateMediaItem(string mediaId, string title, string artist, string mediaUri)
        {
            var description = new MediaDescriptionCompat.Builder()
                .SetMediaId(mediaId)
                .SetTitle(title)
                .SetSubtitle(artist)
                .SetMediaUri(Android.Net.Uri.Parse(mediaUri));

            return new MediaItem(description.Build(), MediaItem.FlagPlayable);
        }


        private class MediaSessionCallback : MediaSessionCompat.Callback
        {
            private string mediaId = string.Empty;
            private MediaItem selectedMediaItem;
            private bool isMediaItemChanged;

            public MediaSessionCallback()
            {
                CrossMediaManager.Current.StateChanged += Current_StateChanged;                
            }
            
            private void Current_StateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
            {                
                
                if(e.State == MediaManager.Player.MediaPlayerState.Playing && isMediaItemChanged)
                {                    
                    SetMetaInfo(selectedMediaItem);
                    SetPlaybackState(selectedMediaItem, 0, PlaybackStateCompat.StatePlaying);
                    isMediaItemChanged = false;
                }
            }

            public override void OnPlay()
            {                
                PlaySong(false);
            }

            public override void OnPause()
            {
                PauseSong();
            }

            public override void OnStop()
            {
                StopSong();
            }

            public override void OnSkipToNext()
            {
                var selectedIndex = mediaItems.FindIndex(m => m.MediaId == selectedMediaItem.MediaId);
                var targetIndex = (selectedIndex + 1) % mediaItems.Count;

                OnPlayFromMediaId(mediaItems[targetIndex].Description.MediaId, null);
            }

            public override void OnSkipToPrevious()
            {
                var selectedIndex = mediaItems.FindIndex(m => m.MediaId == selectedMediaItem.MediaId);
                var targetIndex = (selectedIndex - 1 + mediaItems.Count) % mediaItems.Count;

                OnPlayFromMediaId(mediaItems[targetIndex].Description.MediaId, null);
            }


            public override async void OnPlayFromMediaId(string mediaId, Bundle extras)
            {
                await CrossMediaManager.Current.Stop();
                isMediaItemChanged = true;
                this.mediaId = mediaId;
                PlaySong(true);
            }

            private async void PlaySong(bool isMediaItemChanged)
            {
                if (mediaItems == null || mediaItems.Count == 0)
                    return;                

                if (mediaId == string.Empty)
                    selectedMediaItem = mediaItems[0];
                else
                    selectedMediaItem = mediaItems.Where(m => m.MediaId == mediaId).FirstOrDefault();                

                if(CrossMediaManager.Current.State == MediaManager.Player.MediaPlayerState.Paused && !isMediaItemChanged)
                {
                    long currentPosition = GetCurrentPosition();
                    SetPlaybackState(selectedMediaItem, currentPosition, PlaybackStateCompat.StatePlaying);
                    await CrossMediaManager.Current.Play();
                }
                else
                    await StartPlaying(selectedMediaItem);
            }

            private static long GetCurrentPosition()
            {
                MediaControllerCompat mediaController = mediaSession.Controller;
                PlaybackStateCompat playbackState = mediaController.PlaybackState;
                long currentPosition = playbackState != null ? playbackState.Position : 0;
                return currentPosition;
            }

            private async void PauseSong()
            {
                long currentPosition = GetCurrentPosition();
                SetPlaybackState(selectedMediaItem, currentPosition, PlaybackStateCompat.StatePaused);
                await CrossMediaManager.Current.Pause();                
            }

            private void StopSong()
            {
                var playbackStateBuilder = new PlaybackStateCompat.Builder()
                    .SetState(PlaybackStateCompat.StateStopped, 0, 1.0f);
                
                mediaSession.SetPlaybackState(playbackStateBuilder.Build());                
            }

            
            private async Task StartPlaying(MediaBrowserCompat.MediaItem mediaItem)
            {
                SetMetaInfo(mediaItem, true);
                await CrossMediaManager.Current.Play(mediaItem.Description.MediaUri.ToString());                
            }

            private void SetMetaInfo(MediaItem selectedMediaItem, bool isLoading = false)
            {
                long totalDuration = (long)CrossMediaManager.Current.Duration.TotalMilliseconds;
                long currentPosition = 0;

                MediaMetadataCompat.Builder metadataBuilder = new MediaMetadataCompat.Builder()
                .PutString(MediaMetadataCompat.MetadataKeyDisplayTitle, selectedMediaItem.Description.Title)
                .PutString(MediaMetadataCompat.MetadataKeyDisplaySubtitle, (isLoading)? "Loading..." : selectedMediaItem.Description.Subtitle)
                .PutString(MediaMetadata.MetadataKeyMediaId, selectedMediaItem.Description.MediaId)
                .PutLong(MediaMetadata.MetadataKeyDuration, totalDuration)
                .PutLong(MediaMetadata.MetadataKeyTrackNumber, currentPosition)
                .PutString(MediaMetadataCompat.MetadataKeyAlbumArtUri, selectedMediaItem.Description.IconUri.ToString());

                
                mediaSession.SetMetadata(metadataBuilder.Build());
                SetPlaybackState(selectedMediaItem, currentPosition, PlaybackStateCompat.StatePaused);
            }

            private static void SetPlaybackState(MediaItem selectedMediaItem, long currentPosition, int state)
            {
                Bundle playbackStateExtras = new Bundle();
                playbackStateExtras.PutString(
                    MediaConstants.PlaybackStateExtrasKeyMediaId, selectedMediaItem.Description.MediaId);

                PlaybackStateCompat.Builder playbackStateBuilder = new PlaybackStateCompat.Builder()
                .SetState(state, currentPosition, 1.0f)
                .SetBufferedPosition(SystemClock.UptimeMillis());

                playbackStateBuilder.SetExtras(playbackStateExtras);

                playbackStateBuilder.SetActions(PlaybackStateCompat.ActionPlay | PlaybackStateCompat.ActionPause | PlaybackStateCompat.ActionSkipToNext | PlaybackStateCompat.ActionSkipToPrevious);
                PlaybackStateCompat playbackState = playbackStateBuilder.Build();
                mediaSession.SetPlaybackState(playbackState);
            }            

            private long GetAudioDuration(string audioFilePath)
            {
                MediaMetadataRetriever retriever = new MediaMetadataRetriever();
                retriever.SetDataSource(audioFilePath);
                string durationString = retriever.ExtractMetadata(MetadataKey.Duration);

                if (long.TryParse(durationString, out long duration))
                {
                    return duration;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}