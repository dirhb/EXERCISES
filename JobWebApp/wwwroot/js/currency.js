/* ============================================================
   currency.js — show salaries in the user's chosen currency
   ------------------------------------------------------------
   Salaries are stored and rendered in USD. The user's preferred
   currency is a profile setting, injected by the server as
   window.JOBFIND_CURRENCY (defaults to USD for everyone, incl.
   guests). If it's USD we leave the amounts as-is; otherwise we
   ask the /Currency/Rate proxy for the USD→currency rate and
   convert every <span class="money" data-usd="50000">.
   ============================================================ */

async function convertSalaries() {
    const elements = document.querySelectorAll('.money[data-usd]');
    if (!elements.length) return;

    const locale = navigator.language || 'en-US';
    const currency = (window.JOBFIND_CURRENCY || 'USD').toUpperCase();

    // USD is the default — just normalise the formatting and stop.
    if (currency === 'USD') {
        elements.forEach(el => formatMoney(el, parseFloat(el.dataset.usd), 'USD', locale));
        return;
    }

    let rate = null;
    try {
        const res = await fetch('/Currency/Rate?to=' + encodeURIComponent(currency));
        if (res.ok) {
            const data = await res.json();
            rate = data.rate;
        }
    } catch (e) {
        // proxy/API unavailable — leave USD amounts untouched
    }

    elements.forEach(el => {
        const usd = parseFloat(el.dataset.usd);
        if (isNaN(usd)) return;
        if (rate) {
            formatMoney(el, usd * rate, currency, locale);
            el.title = formatMoney(null, usd, 'USD', locale) + ' (converted from USD)';
        } else {
            formatMoney(el, usd, 'USD', locale);
        }
    });
}

// Formats an amount. If el is provided, writes into it; always returns the string.
function formatMoney(el, amount, currency, locale) {
    let text;
    try {
        text = new Intl.NumberFormat(locale, {
            style: 'currency',
            currency: currency,
            maximumFractionDigits: 0
        }).format(amount);
    } catch (e) {
        text = '$' + Math.round(amount).toLocaleString();
    }
    if (el) el.textContent = text;
    return text;
}

convertSalaries();
