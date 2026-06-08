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
