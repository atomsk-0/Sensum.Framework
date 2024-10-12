(() => {
    "use strict";
    const e = {
        recaptcha_auto_open: 1,
        recaptcha_auto_solve: 1,
        recaptcha_click_delay_time: 300,
        recaptcha_solve_delay_time: 1e3
    };
    chrome.runtime.onInstalled.addListener((async () => {
        for (const [t, a] of Object.entries(e)) {
            const e = await chrome.storage.local.get(t);
            console.log(e), void 0 === e[t] && await chrome.storage.local.set({
                [t]: a
            })
        }
        const t = chrome.runtime.getURL("").startsWith("moz"),
            a = await chrome.permissions.contains({
                origins: ["<all_urls>", "*://*.google.com/recaptcha/*", "*://*.recaptcha.net/recaptcha/*"]
            });
        t && !a && chrome.tabs.create({
            url: chrome.runtime.getURL("setup.html")
        })
    }));
    const t = {};
    chrome.runtime.onMessage.addListener((function({
        type: e,
        label: a
    }, c, s) {
        return (async () => {
            "KV_SET" === e ? (a.tab_specific && (a.key = `${c.tab.id}_${a.key}`), t[a.key] = a.value, s({
                status: "success"
            })) : "KV_GET" === e && (a.tab_specific && (a.key = `${c.tab.id}_${a.key}`), s({
                status: "success",
                value: t[a.key]
            }))
        })(), !0
    }))
})();