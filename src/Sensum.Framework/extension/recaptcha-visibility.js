(()=>{"use strict";class e{static time(){return Date.now||(Date.now=()=>(new Date).getTime()),Date.now()}static sleep(e=1e3){return new Promise((t=>setTimeout(t,e)))}static async random_sleep(t,i){const a=Math.floor(Math.random()*(i-t)+t);return await e.sleep(a)}}class t{static async get({key:e,tab_specific:t}){return new Promise(((i,a)=>{chrome.runtime.sendMessage({type:"KV_GET",label:{key:e,tab_specific:t}},(e=>{e?i(e):a()}))}))}static async set({key:e,value:t,tab_specific:i}){return new Promise(((a,c)=>{chrome.runtime.sendMessage({type:"KV_SET",label:{key:e,value:t,tab_specific:i}},(e=>{e?a(e):c()}))}))}}function i(e,t=null,i=null){if(null===t||null===i){const a=e.getBoundingClientRect();t=a.left+a.width/2,i=a.top+a.height/2}if(isNaN(t)||isNaN(i))return;const a=undefined;["mouseover","mouseenter","mousedown","mouseup","click","mouseout"].forEach((a=>{const c=undefined,s=new MouseEvent(a,{detail:"mouseover"===a?0:1,view:window,bubbles:!0,cancelable:!0,clientX:t,clientY:i});e.dispatchEvent(s)}))}(async()=>{async function i(){const e=document.querySelectorAll('iframe[src*="/recaptcha/api2/bframe"], iframe[src*="/recaptcha/enterprise/bframe"]');for(const i of e)if("visible"===window.getComputedStyle(i).visibility)return await t.set({key:"recaptcha_image_visible",value:!0,tab_specific:!0});if(e.length>0)return await t.set({key:"recaptcha_image_visible",value:!1,tab_specific:!0})}async function a(){const e=document.querySelectorAll('iframe[src*="/recaptcha/api2/anchor"], iframe[src*="/recaptcha/enterprise/anchor"]');for(const i of e)if("visible"===window.getComputedStyle(i).visibility)return await t.set({key:"recaptcha_widget_visible",value:!0,tab_specific:!0});if(e.length>0)return await t.set({key:"recaptcha_widget_visible",value:!1,tab_specific:!0})}for(;;)await e.sleep(1e3),chrome.runtime?.id&&(await i(),await a())})()})();