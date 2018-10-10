var b, b2;
window.ontransitionend = function () { 
var check = document.getElementsByClassName('cutom-music-player-download-button');
if (check.length == 0) {
	/// aesthetic song download button
	b = document.createElement('ytd-subscribe-button-renderer');
	b.className = 'cutom-music-player-download-button';
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
	var bt = document.createTextNode('Song Download');
	bs.append(bt);
	b.removeChild(b.children[0]);
	
	/// aesthetic video download button
	b2 = document.createElement('ytd-subscribe-button-renderer');
	b2.className = 'cutom-music-player-download-button';
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
	var bt2 = document.createTextNode('Video Download');
	bs2.append(bt2);
	b2.removeChild(b2.children[0]);
	
	/// add buttons to document
	var container = document.getElementsByClassName('style-scope ytd-video-secondary-info-renderer')[0];
	container.appendChild(container.children[1].cloneNode());
	container.children[3].appendChild(b);
	container.children[3].appendChild(b2);
	updateButtonSize();
	window.addEventListener('resize', updateButtonSize);
	
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