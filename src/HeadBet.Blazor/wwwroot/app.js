// HeadBet Blazor - helpers JS

// Rola até o elemento indicado pelo fragmento (#anchor) da URL atual.
// Tenta algumas vezes porque, numa aba recém-aberta, o Blazor Server pode
// renderizar a seção alguns ms depois do carregamento da página.
window.hbScrollToHash = function () {
    const hash = window.location.hash;
    if (!hash || hash.length < 2) return;

    const id = decodeURIComponent(hash.substring(1));
    let tries = 0;
    const tick = () => {
        const el = document.getElementById(id);
        if (el) {
            el.scrollIntoView({ behavior: 'smooth', block: 'start' });
        } else if (tries++ < 20) {
            setTimeout(tick, 50);
        }
    };
    tick();
};

// Rola o calendário de jogos horizontalmente até a coluna do dia indicado.
// O .hb-calendar tem position:relative, então offsetLeft já é relativo a ele.
window.hbCalendarScrollToDay = function (id) {
    const el = document.getElementById(id);
    if (!el) return;
    const container = el.closest('.hb-calendar');
    if (!container) return;
    container.scrollTo({ left: el.offsetLeft, behavior: 'smooth' });
};

// Baixa um conteúdo de texto como arquivo (usado pelo Console SQL: JSON/CSV).
window.hbDownloadFile = function (filename, content, mime) {
    const blob = new Blob([content], { type: mime || 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};
