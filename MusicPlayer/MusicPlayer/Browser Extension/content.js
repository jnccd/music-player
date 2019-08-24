var b, b2;
window.ontransitionend = function () { 
var check = document.getElementsByClassName('custom-music-player-download-button');
if (check.length == 0) {
    
	/// aesthetic song download button
	b = document.createElement('ytd-subscribe-button-renderer');
	b.className = 'custom-music-player-download-button';
	b.setAttribute('is-icon-button', '');
	b.addEventListener("click", function(){
		var video = document.querySelector( 'video' );
		if (video) 
		{
			video.pause();
			var downloadstring = window.location.href + '±' + video.currentTime;
			download('MusicPlayer.PlayRequest', downloadstring);
		}
	});
	var bp = document.createElement('paper-button');
	bp.className = 'style-scope ytd-subscribe-button-renderer';
	b.append(bp);
	var bs = document.createElement('yt-formatted-string');
	bs.id = 'text';
	bs.className = 'style-scope ytd-subscribe-button-renderer';
	bp.append(bs);
	
	/// aesthetic video download button
	b2 = document.createElement('ytd-subscribe-button-renderer');
	b2.className = 'custom-music-player-download-button';
	b2.setAttribute('is-icon-button', '');
	b2.addEventListener("click", function(){
		var video = document.querySelector( 'video' );
		if (video) 
		{
			video.pause();
			var downloadstring = window.location.href;
			download('MusicPlayer.VideoDownloadRequest', downloadstring);
		}
	});
	var bp2 = document.createElement('paper-button');
	bp2.className = 'style-scope ytd-subscribe-button-renderer';
	b2.append(bp2);
	var bs2 = document.createElement('yt-formatted-string');
	bs2.id = 'text';
	bs2.className = 'style-scope ytd-subscribe-button-renderer';
	bp2.append(bs2);
    
	/// add buttons to document
	var container = document.getElementsByClassName('style-scope ytd-video-secondary-info-renderer')[0];
	container.appendChild(container.children[1].cloneNode());
	container.children[3].appendChild(b);
	container.children[3].appendChild(b2);
	updateButtonSize();
	window.addEventListener('resize', updateButtonSize);
    
    /// post add adjustments
    b.children[0].style.height = '35px';
	b2.children[0].style.height = b.children[0].style.height;
    b.children[0].append(document.createTextNode("Song Download"));
    b2.children[0].append(document.createTextNode("Video Download"));
	
	console.log('MusicPlayer buttons added!');
	}
}

function updateButtonSize() {
    var p = document.getElementById('primary-inner');
	b.children[1].style.width = (p.offsetWidth/2 - 8)+'px';
	b2.children[1].style.width = b.children[1].style.width;
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