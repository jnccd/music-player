var buttons = [];

window.ontransitionend = function () { 
  var check = document.getElementsByClassName('custom-music-player-download-button');
  if (check.length == 0) {
    
	/// song download button
    addButton("Song Download", function(){
		var video = document.querySelector( 'video' );
		if (video) 
		{
			video.pause();
			var downloadstring = window.location.href + '±' + video.currentTime;
			download('MusicPlayer.PlayRequest', downloadstring);
		}
	});
	
	/// video download button
    addButton("Video Download", function(){
		var video = document.querySelector('video');
		if (video) 
		{
			video.pause();
			var downloadstring = window.location.href;
			download('MusicPlayer.VideoDownloadRequest', downloadstring);
		}
	});
    
    /// view thumbnail button
    addButton("View Thumbnail", function(){
        var videoID = window.location.search.split('v=')[1].split('&')[0];
		window.open(`https://img.youtube.com/vi/${videoID}/maxresdefault.jpg`);
	});
	
    /// properly resize ma bois
    updateButtonSize();
	window.addEventListener('resize', updateButtonSize);
    
	console.log('MusicPlayer buttons added!');
  }
}

function addButton(text, clickEvent)
{
    /// prepare button object
    var b = document.createElement('ytd-subscribe-button-renderer');
	b.className = 'custom-music-player-download-button';
	b.setAttribute('is-icon-button', '');
	b.addEventListener("click", function() {
      clickEvent();
    });
	var bp = document.createElement('paper-button');
	bp.className = 'style-scope ytd-subscribe-button-renderer';
	b.append(bp);
	var bs = document.createElement('yt-formatted-string');
	bs.id = 'text';
	bs.className = 'style-scope ytd-subscribe-button-renderer';
	bp.append(bs);
    
    /// add button to document
    var container = document.getElementsByClassName('style-scope ytd-video-secondary-info-renderer')[0];
    if (container.children.length < 4)
        container.appendChild(container.children[1].cloneNode());
	container.children[3].appendChild(b);
    
    /// post add adjustments
    b.children[0].style.height = '35px';
    b.children[0].append(document.createTextNode(text));
    
    buttons[buttons.length] = b;
}

function updateButtonSize() {
    var p = document.getElementById('primary-inner');
    
    for (var i = 0; i < buttons.length; i++) {
        buttons[i].style.width = (p.offsetWidth/buttons.length - 8)+'px';
        buttons[i].children[0].style.width = (p.offsetWidth/buttons.length - 8)+'px';
    }
}

function download(filename, text) {
    var pom = document.createElement('a');
    pom.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(text));
    pom.setAttribute('download', filename);

    if (document.createEvent) {
        var event = document.createEvent('MouseEvents');
        event.initEvent('click', true, true);
        pom.dispatchEvent(event);
    }
    else {
        pom.click();
    }
}