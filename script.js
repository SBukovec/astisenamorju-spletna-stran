const v = window.VSEBINA || {};
const text = (id, value) => { const e = document.getElementById(id); if (e) e.textContent = value || ''; };
const link = (id, label, url) => { const e = document.getElementById(id); if (!e) return; e.textContent = label || ''; e.href = url || '#'; };
const image = (id, src, alt) => { const e = document.getElementById(id); if (!e) return; e.alt = alt || ''; e.hidden = !src; if (src) e.src = src; else e.removeAttribute('src'); };
const imageDisplay = (element, position, size, fit) => {
  if (!element) return;
  const percent = Math.max(100, Math.min(300, Number(size) || 100));
  element.style.objectPosition = position || 'center center';
  element.style.objectFit = fit === 'contain' ? 'contain' : 'cover';
  element.style.transform = `scale(${percent / 100})`;
};

document.title = v.naslovStrani || v.imeBenda;
document.getElementById('meta-description').content = v.metaOpis || '';
document.documentElement.style.setProperty('--night', v.barvaOzadja || '#171713');
document.documentElement.style.setProperty('--paper', v.barvaPapirja || '#f0eadf');
document.documentElement.style.setProperty('--copper', v.barvaPoudarka || '#d56f3e');
const headerBg = (v.glavaBarvaOzadja || '').trim().toLowerCase();
const headerFg = (v.glavaBarvaBesedila || '').trim().toLowerCase();
if (headerBg === '' || headerBg === 'prosojno' || headerBg === 'prazno' || headerBg === 'brez') {
  document.documentElement.style.setProperty('--header-bg', 'transparent');
  document.documentElement.style.setProperty('--header-bg-mobile', 'rgba(17,16,14,.91)');
} else {
  document.documentElement.style.setProperty('--header-bg', headerBg);
  document.documentElement.style.setProperty('--header-bg-mobile', headerBg);
}
if (headerFg === '' || headerFg === 'izvorno' || headerFg === 'prazno') document.documentElement.style.removeProperty('--header-fg');
else document.documentElement.style.setProperty('--header-fg', headerFg);
text('site-wordmark', v.imeBenda); image('site-logo', v.logotip, v.imeBenda); image('site-wordmark-img', v.imeGrafika, v.imeBenda);
const showGrafika = (v.headerMode || '').trim().toLowerCase() === 'grafika' && !!v.imeGrafika;
const wordmarkEl = document.getElementById('site-wordmark-img');
const wordmarkScale = Math.max(30, Math.min(250, Number(v.imeGrafikaVelikost) || 100)) / 100;
wordmarkEl.hidden = !showGrafika;
wordmarkEl.style.height = `${Math.round(70 * wordmarkScale)}px`;
wordmarkEl.style.width = 'auto';
wordmarkEl.style.maxWidth = `${Math.round(440 * wordmarkScale)}px`;
wordmarkEl.style.marginLeft = `${Number(v.imeGrafikaOdmikX) || 0}px`;
wordmarkEl.style.marginTop = `${Number(v.imeGrafikaOdmikY) || 0}px`;
document.getElementById('site-wordmark').hidden = showGrafika;

const nav = document.getElementById('main-nav');
for (let i = 1; i <= 9; i++) if (v[`menu${i}`]) {
  const a = document.createElement('a'); a.textContent = v[`menu${i}`]; a.href = v[`menu${i}Link`] || '#'; nav.appendChild(a);
}

['eyebrow','naslov','poudarek','uvodniOpis'].forEach((k, i) => text(['hero-eyebrow','hero-title','hero-highlight','hero-lead'][i], v[k]));
link('hero-button-one', v.heroGumb1, v.heroGumb1Link); link('hero-button-two', v.heroGumb2, v.heroGumb2Link); text('scroll-note', v.scrollTekst);
const video = document.getElementById('hero-video'); const source = document.getElementById('hero-video-source'); source.src = v.video || ''; video.poster = v.videoPoster || ''; video.load();
video.style.objectPosition = v.videoPozicija || 'center center';

text('story-index',v.storyIndex); text('story-title',v.storyTitle); text('story-one',v.zgodba1); text('story-two',v.zgodba2); image('story-image',v.storySlika,v.storySlikaAlt); imageDisplay(document.getElementById('story-image'),v.storySlikaPozicija,v.storySlikaVelikost,v.storySlikaNacin);
const lineup = document.getElementById('lineup'); [v.zasedba1,v.zasedba2,v.zasedba3,v.zasedba4].filter(Boolean).forEach(x => { const s=document.createElement('span'); s.textContent=x; lineup.appendChild(s); });

text('music-index',v.musicIndex); text('music-title',v.musicTitle); text('music-description',v.musicDescription); text('music-note',v.musicNote);
const songs = document.getElementById('song-list');
for (let i=1;i<=2;i++) { const article=document.createElement('article'); article.className='song'; const url=v[`skladba${i}Link`]; article.innerHTML=`<span class="song-no">0${i}</span><div><h3></h3><p></p></div><a class="play" aria-label="Odpri skladbo">▶</a>`; article.querySelector('h3').textContent=v[`skladba${i}`]||''; article.querySelector('p').textContent=v[`skladba${i}Opis`]||''; const a=article.querySelector('a'); a.href=url||'#glasba'; if(!url) a.classList.add('disabled'); songs.appendChild(article); }

text('live-index',v.liveIndex); text('live-title',v.liveTitle); text('live-description',v.liveDescription); link('live-link',v.liveLinkText,v.liveLinkUrl); image('live-image',v.liveSlika,v.liveSlikaAlt); imageDisplay(document.getElementById('live-image'),v.liveSlikaPozicija,v.liveSlikaVelikost,v.liveSlikaNacin); text('live-caption',v.liveCaption);
text('gallery-index',v.galleryIndex); text('gallery-title',v.galleryTitle); text('gallery-description',v.galleryDescription);
const grid=document.getElementById('gallery-grid');
const galleryPages=document.getElementById('gallery-pages');
const galleryItems=Array.isArray(v.galerija)&&v.galerija.length?v.galerija:[1,2,3].filter(i=>v[`gallery${i}`]).map(i=>({slika:v[`gallery${i}`],opis:v[`gallery${i}Alt`]||'',pozicija:v[`gallery${i}Pozicija`]||'center center',velikost:'100',nacin:'cover'}));
let perPage=5;
let currentGalleryPage=0;
function showGalleryPage(page){
  const pageCount=Math.max(1,Math.ceil(galleryItems.length/perPage)); currentGalleryPage=Math.min(page,pageCount-1);
  grid.innerHTML=''; const pageItems=galleryItems.slice(currentGalleryPage*perPage,currentGalleryPage*perPage+perPage); grid.className=`gallery-grid count-${pageItems.length}`; galleryPages.querySelectorAll('button').forEach((b,i)=>b.classList.toggle('active',i===currentGalleryPage));
  pageItems.forEach(item=>{
    const figure=document.createElement('figure'); const img=document.createElement('img');
    const ratio=Math.max(.55,Math.min(2.2,Number(item.razmerje)||1.35)); figure.style.setProperty('--photo-ratio',String(ratio));
    img.src=item.slika; img.alt=item.opis||''; imageDisplay(img,item.pozicija,item.velikost,item.nacin); img.loading='lazy'; figure.appendChild(img);
    if(item.opis){const caption=document.createElement('figcaption');caption.textContent=item.opis;figure.appendChild(caption);} grid.appendChild(figure);
  });
}
function layoutGallery(){
  const width=grid.clientWidth||window.innerWidth; const targetHeight=280; const mobile=width<760;
  if(mobile){perPage=galleryItems.length||1;currentGalleryPage=0;}
  else{
    let rows=1,rowWidth=0,nextPerPage=0;
    for(const item of galleryItems){
      const ratio=Math.max(.55,Math.min(2.2,Number(item.razmerje)||1.35)); const cardWidth=Math.max(170,Math.min(610,ratio*targetHeight));
      if(rowWidth>0&&rowWidth+12+cardWidth>width){rows+=1;rowWidth=0;} if(rows>2)break;
      rowWidth+=(rowWidth?12:0)+cardWidth;nextPerPage+=1;
    }
    nextPerPage=Math.max(1,nextPerPage);
    if(nextPerPage!==perPage){perPage=nextPerPage;currentGalleryPage=0;}
  }
  const pageCount=Math.max(1,Math.ceil(galleryItems.length/perPage));
  galleryPages.innerHTML='';
  if(pageCount>1) for(let p=0;p<pageCount;p++){const b=document.createElement('button');b.type='button';b.textContent=String(p+1);b.setAttribute('aria-label',`Galerija, stran ${p+1}`);b.addEventListener('click',()=>showGalleryPage(p));galleryPages.appendChild(b);}
  showGalleryPage(currentGalleryPage);
}
let galleryResizeTimer; window.addEventListener('resize',()=>{clearTimeout(galleryResizeTimer);galleryResizeTimer=setTimeout(layoutGallery,120);});
layoutGallery();
Promise.all(galleryItems.map(item=>new Promise(resolve=>{const probe=new Image();probe.onload=()=>{item.razmerje=probe.naturalWidth/probe.naturalHeight;resolve();};probe.onerror=resolve;probe.src=item.slika;}))).then(layoutGallery);

text('video-index',v.videoIndex); text('video-title',v.videoTitle); text('video-description',v.videoDescription);
const videoGrid=document.getElementById('video-grid');
(Array.isArray(v.videoteka)?v.videoteka:[]).forEach(item=>{
  if(String(item.aktiven||'').trim().toLowerCase()==='ne') return;
  const src=(item.video||'').trim(); if(!src) return;
  const fig=document.createElement('figure'); fig.className='video-card';
  const isUrl=/^https?:\/\//i.test(src);
  if(!isUrl){
    const vid=document.createElement('video'); vid.controls=true; vid.preload='metadata'; vid.playsInline=true;
    const s=document.createElement('source'); s.src=src; s.type=src.toLowerCase().endsWith('.webm')?'video/webm':'video/mp4';
    vid.appendChild(s); fig.appendChild(vid);
  } else if(src.includes('youtube.com')||src.includes('youtu.be')){
    const m=src.match(/(?:youtube\.com\/(?:watch\?v=|embed\/|shorts\/)|youtu\.be\/)([\w-]{11})/);
    const ifr=document.createElement('iframe'); ifr.src=m?`https://www.youtube.com/embed/${m[1]}`:src; ifr.setAttribute('allowfullscreen',''); ifr.loading='lazy'; ifr.title='Video';
    fig.appendChild(ifr);
  } else if(src.includes('drive.google.com')){
    const m=src.match(/\/file\/d\/([^/?#]+)/);
    const ifr=document.createElement('iframe'); ifr.src=m?`https://drive.google.com/file/d/${m[1]}/preview`:src; ifr.setAttribute('allowfullscreen',''); ifr.loading='lazy'; ifr.title='Video';
    fig.appendChild(ifr);
  } else if(/\.(mp4|webm|ogg|mov)(\?|$)/i.test(src)){
    const vid=document.createElement('video'); vid.controls=true; vid.preload='metadata'; vid.playsInline=true;
    const s=document.createElement('source'); s.src=src; s.type=src.toLowerCase().endsWith('.webm')?'video/webm':'video/mp4';
    vid.appendChild(s); fig.appendChild(vid);
  } else {
    const a=document.createElement('a'); a.className='video-external'; a.href=src; a.target='_blank'; a.rel='noopener'; a.textContent='Oglej si video ↗'; fig.appendChild(a);
  }
  if(item.naslov){const cap=document.createElement('figcaption'); cap.textContent=item.naslov; fig.appendChild(cap);}
  videoGrid.appendChild(fig);
});

text('contact-index',v.contactIndex); text('contact-title',v.contactTitle); text('contact-highlight',v.contactHighlight); text('contact-description',v.contactDescription); link('contact-email',v.kontakt,`mailto:${v.kontakt}`);
const socials=document.getElementById('socials'); ['facebook','instagram','youtube'].forEach(k=>{if(v[`${k}Text`]){const a=document.createElement('a');a.textContent=v[`${k}Text`];a.href=v[`${k}Url`]||'#kontakt';if(!v[`${k}Url`])a.classList.add('disabled');socials.appendChild(a);}});
text('footer-left',v.footerLeft); text('footer-right',v.footerRight);

const menuButton=document.querySelector('.menu-button'); menuButton.addEventListener('click',()=>{const open=nav.classList.toggle('open');menuButton.setAttribute('aria-expanded',String(open));}); nav.querySelectorAll('a').forEach(a=>a.addEventListener('click',()=>{nav.classList.remove('open');menuButton.setAttribute('aria-expanded','false');}));
const observer=new IntersectionObserver(entries=>entries.forEach(e=>{if(e.isIntersecting)e.target.classList.add('visible')}),{threshold:.12}); document.querySelectorAll('.reveal').forEach(e=>observer.observe(e));
