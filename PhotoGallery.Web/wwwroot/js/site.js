// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("quickSearchForm");
    const input = document.getElementById("quickSearchInput");
    const results = document.getElementById("quickSearchResults");

    if (!form || !input || !results) return;

    async function runSearch() {
        const data = new FormData(form);

    const response = await fetch("/Gallery/QuickSearch", {
        method: "POST",
    body: data
        });

    results.innerHTML = await response.text();
    }

    // 1) Press Enter -> prevent full page post -> AJAX search
    form.addEventListener("submit", async (e) => {
        e.preventDefault();
    await runSearch();
    });

    // 2) Optional: live search as you type (with small debounce)
    let timer = null;
    input.addEventListener("input", () => {
        clearTimeout(timer);
    timer = setTimeout(runSearch, 250);
    });

    // 3) Clear results when modal opens
    const modalEl = document.getElementById("quickSearchModal");
    modalEl.addEventListener("shown.bs.modal", () => {
        input.focus();
    results.innerHTML = "";
    });
});