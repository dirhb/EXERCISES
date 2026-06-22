/* ============================================================
   report.js — contextual "Report" modal
   ------------------------------------------------------------
   Any .report-btn carries data-report-type ("Job"/"Applicant"),
   data-report-id, and data-report-label. Clicking it fills the
   shared #report-modal and opens it. The modal posts to
   /Report/Submit and returns to the current page.
   ============================================================ */
(function () {
    const modal = document.getElementById('report-modal');
    if (!modal) return;

    const form = modal.querySelector('#report-form');
    const overlay = modal.querySelector('.modal-overlay');
    const cancelBtn = modal.querySelector('.report-cancel');
    const label = modal.querySelector('.report-target-label');
    const typeInput = form.querySelector('[name="targetType"]');
    const idInput = form.querySelector('[name="targetId"]');
    const subjInput = form.querySelector('[name="subject"]');
    const returnInput = form.querySelector('[name="returnUrl"]');

    function openModal(btn) {
        typeInput.value = btn.dataset.reportType || '';
        idInput.value = btn.dataset.reportId || '';
        subjInput.value = btn.dataset.reportLabel || '';
        returnInput.value = window.location.pathname + window.location.search;
        if (label) label.textContent = btn.dataset.reportLabel ? ('"' + btn.dataset.reportLabel + '"') : 'this';
        modal.classList.add('open');
        modal.setAttribute('aria-hidden', 'false');
    }

    function closeModal() {
        modal.classList.remove('open');
        modal.setAttribute('aria-hidden', 'true');
    }

    document.querySelectorAll('.report-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            openModal(btn);
        });
    });

    if (cancelBtn) cancelBtn.addEventListener('click', closeModal);
    if (overlay) overlay.addEventListener('click', closeModal);
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.classList.contains('open')) closeModal();
    });
})();
