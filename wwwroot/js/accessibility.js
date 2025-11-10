(function () {
    const body = document.body;
    const html = document.documentElement;

    // UI
    const ui = {
        fab: document.getElementById('a11yFab'),
        panel: document.getElementById('a11yPanel'),
        close: document.getElementById('a11yClose'),
        apply: document.getElementById('a11yApply'),
        fontRange: document.getElementById('a11yFontRange'),
        fontValue: document.getElementById('a11yFontValue'),
        cvdMode: document.getElementById('a11yCvdMode'),
        highContrast: document.getElementById('a11yHighContrast'),
        underlineLinks: document.getElementById('a11yUnderlineLinks'),
        dyslexiaFont: document.getElementById('a11yDyslexiaFont'),
        reduceMotion: document.getElementById('a11yReduceMotion'),
        resetBtn: document.getElementById('a11yReset'),
        voiceSelect: document.getElementById('a11yVoiceSelect'),
        ttsReadSelection: document.getElementById('ttsReadSelection'),
        ttsReadPage: document.getElementById('ttsReadPage'),
        ttsPause: document.getElementById('ttsPause'),
        ttsResume: document.getElementById('ttsResume'),
        ttsStop: document.getElementById('ttsStop'),
        ttsStatus: document.getElementById('ttsStatus'),
        mainContent: document.getElementById('mainContent')
    };

    const defaultSettings = {
        fontScale: 1.0,
        cvdMode: 'none',
        highContrast: false,
        underlineLinks: false,
        dyslexiaFont: false,
        reduceMotion: false,
        voice: null
    };

    function loadSettings() {
        try {
            const raw = localStorage.getItem('a11ySettings');
            return raw ? { ...defaultSettings, ...JSON.parse(raw) } : { ...defaultSettings };
        } catch { return { ...defaultSettings }; }
    }
    function saveSettings(s) { localStorage.setItem('a11ySettings', JSON.stringify(s)); }

    function applySettings(s) {
        // Escala de fuente
        html.style.setProperty('--a11y-font-scale', s.fontScale);
        if (ui.fontRange) ui.fontRange.value = s.fontScale;
        if (ui.fontValue) ui.fontValue.textContent = `${Math.round(s.fontScale * 100)}%`;

        // Daltonismo
        body.classList.remove('a11y-cvd-prot', 'a11y-cvd-deut', 'a11y-cvd-trit');
        if (s.cvdMode === 'prot') body.classList.add('a11y-cvd-prot');
        if (s.cvdMode === 'deut') body.classList.add('a11y-cvd-deut');
        if (s.cvdMode === 'trit') body.classList.add('a11y-cvd-trit');
        if (ui.cvdMode) ui.cvdMode.value = s.cvdMode;

        // Toggles
        setToggle('a11y-high-contrast', s.highContrast, ui.highContrast);
        setToggle('a11y-underline-links', s.underlineLinks, ui.underlineLinks);
        setToggle('a11y-dyslexia-font', s.dyslexiaFont, ui.dyslexiaFont);
        setToggle('a11y-reduce-motion', s.reduceMotion, ui.reduceMotion);

        // Voz
        if (ui.voiceSelect && s.voice) {
            const found = Array.from(ui.voiceSelect.options).find(o => o.value === s.voice);
            if (found) ui.voiceSelect.value = s.voice;
        }
    }
    function setToggle(cls, enabled, checkbox) {
        body.classList.toggle(cls, !!enabled);
        if (checkbox) checkbox.checked = !!enabled;
    }

    // Panel open/close
    function openPanel() {
        if (!ui.panel) return;
        ui.panel.hidden = false;
        ui.fab?.setAttribute('aria-expanded', 'true');
        setTimeout(() => ui.fontRange?.focus(), 0);
    }
    function closePanel() {
        if (!ui.panel) return;
        ui.panel.hidden = true;
        ui.fab?.setAttribute('aria-expanded', 'false');
        ui.fab?.focus();
    }

    let settings = loadSettings();

    // Wire events
    document.addEventListener('DOMContentLoaded', () => {
        // Inicializa UI con settings
        applySettings(settings);

        // FAB toggle
        ui.fab?.addEventListener('click', () => {
            if (ui.panel.hidden) openPanel(); else closePanel();
        });
        ui.close?.addEventListener('click', closePanel);
        ui.apply?.addEventListener('click', () => { saveSettings(settings); closePanel(); });
        // ESC para cerrar
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape' && !ui.panel.hidden) { e.preventDefault(); closePanel(); }
        });
        // Click fuera del panel (área exterior)
        document.addEventListener('click', (e) => {
            if (!ui.panel || ui.panel.hidden) return;
            const isInside = ui.panel.contains(e.target) || ui.fab.contains(e.target);
            if (!isInside) closePanel();
        });

        // Controles
        ui.fontRange?.addEventListener('input', e => {
            settings.fontScale = parseFloat(e.target.value || '1');
            applySettings(settings);
            saveSettings(settings);
        });
        ui.cvdMode?.addEventListener('change', e => {
            settings.cvdMode = e.target.value;
            applySettings(settings);
            saveSettings(settings);
        });
        bindToggle(ui.highContrast, 'highContrast', 'a11y-high-contrast');
        bindToggle(ui.underlineLinks, 'underlineLinks', 'a11y-underline-links');
        bindToggle(ui.dyslexiaFont, 'dyslexiaFont', 'a11y-dyslexia-font');
        bindToggle(ui.reduceMotion, 'reduceMotion', 'a11y-reduce-motion');

        ui.resetBtn?.addEventListener('click', () => {
            settings = { ...defaultSettings };
            applySettings(settings);
            saveSettings(settings);
            speakStatus('Preferencias de accesibilidad restablecidas.');
        });

        // TTS
        initVoices();
        ui.voiceSelect?.addEventListener('change', () => {
            settings.voice = ui.voiceSelect.value || null;
            saveSettings(settings);
        });
        ui.ttsReadSelection?.addEventListener('click', readSelection);
        ui.ttsReadPage?.addEventListener('click', readPage);
        ui.ttsPause?.addEventListener('click', pauseTts);
        ui.ttsResume?.addEventListener('click', resumeTts);
        ui.ttsStop?.addEventListener('click', stopTts);
    });

    function bindToggle(el, key, cls) {
        el?.addEventListener('change', () => {
            settings[key] = !!el.checked;
            applySettings(settings);
            saveSettings(settings);
        });
    }

    // ---- Texto a voz (Web Speech API) ----
    let voices = [];
    function initVoices() {
        if (!('speechSynthesis' in window)) {
            setTtsUIEnabled(false);
            ui.ttsStatus && (ui.ttsStatus.textContent = 'TTS no soportado en este navegador.');
            return;
        }
        function populate() {
            voices = window.speechSynthesis.getVoices();
            if (!ui.voiceSelect) return;
            ui.voiceSelect.innerHTML = '';
            voices
                .filter(v => v.lang.startsWith('es') || v.lang.startsWith('en'))
                .forEach(v => {
                    const opt = document.createElement('option');
                    opt.value = v.name;
                    opt.textContent = `${v.name} (${v.lang})`;
                    ui.voiceSelect.appendChild(opt);
                });
            if (settings.voice) {
                const found = Array.from(ui.voiceSelect.options).find(o => o.value === settings.voice);
                if (found) ui.voiceSelect.value = settings.voice;
            } else if (ui.voiceSelect.options.length) {
                ui.voiceSelect.selectedIndex = 0;
            }
        }
        populate();
        window.speechSynthesis.onvoiceschanged = populate;
        setTtsUIEnabled(true);
    }
    function setTtsUIEnabled(enabled) {
        [ui.ttsReadSelection, ui.ttsReadPage, ui.ttsPause, ui.ttsResume, ui.ttsStop, ui.voiceSelect]
            .forEach(x => x && (x.disabled = !enabled));
    }

    let utterance = null;
    function getSelectedVoice() {
        if (!voices.length) return null;
        const name = ui.voiceSelect && ui.voiceSelect.value;
        return voices.find(v => v.name === name) || voices[0];
    }
    function speak(text) {
        if (!('speechSynthesis' in window) || !text) return;
        stopTts();
        utterance = new SpeechSynthesisUtterance(text);
        const v = getSelectedVoice();
        if (v) utterance.voice = v;
        utterance.rate = 1;
        utterance.pitch = 1;
        utterance.onstart = () => speakStatus('Leyendo…');
        utterance.onend = () => speakStatus('Lectura finalizada.');
        utterance.onerror = () => speakStatus('Error de lectura.');
        window.speechSynthesis.speak(utterance);
    }
    function readSelection(e) {
        e.preventDefault();
        const text = (window.getSelection()?.toString() || '').trim();
        speak(text || 'No hay texto seleccionado.');
    }
    function readPage(e) {
        e.preventDefault();
        const source = ui.mainContent || document.body;
        const text = source.innerText.replace(/\s+/g, ' ').trim();
        speak(text.substring(0, 50000));
    }
    function pauseTts(e) {
        e.preventDefault();
        if (window.speechSynthesis.speaking && !window.speechSynthesis.paused) {
            window.speechSynthesis.pause();
            speakStatus('Pausado.');
        }
    }
    function resumeTts(e) {
        e.preventDefault();
        if (window.speechSynthesis.paused) {
            window.speechSynthesis.resume();
            speakStatus('Reanudado.');
        }
    }
    function stopTts(e) {
        if (e) e.preventDefault();
        if (window.speechSynthesis.speaking || window.speechSynthesis.paused) {
            window.speechSynthesis.cancel();
            speakStatus('Detenido.');
        }
    }
    function speakStatus(msg) { ui.ttsStatus && (ui.ttsStatus.textContent = msg); }
})();