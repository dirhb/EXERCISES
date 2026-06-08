/* ============================================================
   main.js — JobFind shared JavaScript
   ============================================================ */


/* ── 1. ACCORDION (Online Resume page only) ──────────────────
   Finds all accordion section buttons.
   When one is clicked: closes all sections first,
   then opens the clicked one (unless it was already open).
--------------------------------------------------------------- */
if (document.querySelector('.accordion')) {
    document.querySelectorAll('.accordion-header').forEach(btn => {
        btn.addEventListener('click', () => {
            const item = btn.closest('.accordion-item');
            const body = item.querySelector('.accordion-body');
            const isOpen = btn.getAttribute('aria-expanded') === 'true';

            // Close every section
            document.querySelectorAll('.accordion-item').forEach(i => {
                i.querySelector('.accordion-header').setAttribute('aria-expanded', 'false');
                i.querySelector('.accordion-body').hidden = true;
                i.classList.remove('open');
            });

            // Open the clicked one — only if it wasn't already open
            if (!isOpen) {
                btn.setAttribute('aria-expanded', 'true');
                body.hidden = false;
                item.classList.add('open');
            }
        });
    });
}


/* ── 2. NAVIGATION DROPDOWNS ─────────────────────────────────
   Adds keyboard support (Enter/Space to toggle)
--------------------------------------------------------------- */
document.querySelectorAll('.notification-icon, .account-icon').forEach(btn => {
    btn.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            const dropdown = btn.parentElement.querySelector('.dropdown');
            const isVisible = dropdown.style.display === 'block';
            dropdown.style.display = isVisible ? 'none' : 'block';
        }
    });
});


/* ── 3. CLOSE DROPDOWNS WHEN CLICKING OUTSIDE ────────────────
   If the user clicks anywhere outside a dropdown, close all dropdowns.
--------------------------------------------------------------- */
document.addEventListener('click', (e) => {
    if (!e.target.closest('.notification') && !e.target.closest('.account')) {
        document.querySelectorAll('.dropdown').forEach(d => {
            d.style.display = 'none';
        });
    }
});


/* ── 4. REGISTER PAGE — TAB SWITCHER ─────────────────────────
   Switches between Register and Sign In forms.
   Only runs on the Register/Login page.
--------------------------------------------------------------- */
if (document.querySelector('.auth-tab')) {
    const tabs = document.querySelectorAll('.auth-tab');
    const registerForm = document.getElementById('register-form');
    const signinForm = document.getElementById('signin-form');

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            // Remove "active" from all tabs first
            tabs.forEach(t => t.classList.remove('active'));

            // Add "active" to the tab that was clicked
            tab.classList.add('active');

            // Show the correct form
            if (tab.dataset.tab === 'register') {
                registerForm.classList.remove('hidden');
                signinForm.classList.add('hidden');
            } else {
                signinForm.classList.remove('hidden');
                registerForm.classList.add('hidden');
            }
        });
    });
}


/* ── 5. REGISTER PAGE — PASSWORD SHOW/HIDE ───────────────────
   Clicking the eye icon toggles password visibility.
--------------------------------------------------------------- */
if (document.querySelector('.toggle-pw')) {
    document.querySelectorAll('.toggle-pw').forEach(btn => {
        btn.addEventListener('click', () => {
            const input = btn.closest('.auth-input-wrap').querySelector('input');
            const icon = btn.querySelector('.material-symbols-outlined');

            if (input.type === 'password') {
                input.type = 'text';
                icon.textContent = 'visibility_off';
            } else {
                input.type = 'password';
                icon.textContent = 'visibility';
            }
        });
    });
}


/* ── 6. ROLE TOGGLE (Employee / Employer) ────────────────────
   Highlights the selected role label on the register form.
--------------------------------------------------------------- */
if (document.querySelector('.role-label')) {
    document.querySelectorAll('input[name="UserTypeID"]').forEach(radio => {
        radio.addEventListener('change', () => {
            // Remove active style from all role labels
            document.querySelectorAll('.role-label').forEach(label => {
                label.style.color = '';
                label.style.background = '';
                label.parentElement.style.background = '';
                label.parentElement.style.borderRadius = '';
                label.parentElement.style.boxShadow = '';
            });

            // Highlight the selected one
            const selectedLabel = radio.nextElementSibling;
            selectedLabel.parentElement.style.background = 'var(--surface)';
            selectedLabel.parentElement.style.borderRadius = '999px';
            selectedLabel.parentElement.style.boxShadow = 'var(--shadow-sm)';
        });
    });
}


/* ── 7. DYNAMIC DISPLAY — POLLING (הצגה דינאמית) ────────────
   Every 10 seconds, fetch new notifications and update the dropdown.
   Right now uses fake data — replace getFakeNotifications() with
   a real fetch() call when you have the API endpoint ready.
--------------------------------------------------------------- */
function getFakeNotifications() {
    const all = [
        '3 unread messages',
        '2 interview reminders',
        '1 new saved job match',
        'New job alert: Frontend Developer',
        'Your resume was viewed by Acme Logistics',
        'BlueWave Services replied to your message',
    ];
    const count = Math.floor(Math.random() * 3) + 2;
    return all.slice(0, count);
}

function pollNotifications() {
    const dropdown = document.querySelector('.notification .dropdown');
    if (!dropdown) return;

    const notifications = getFakeNotifications();
    dropdown.innerHTML = '';

    notifications.forEach(message => {
        const li = document.createElement('li');
        li.textContent = message;
        dropdown.appendChild(li);
    });

    console.log('Polling ran — notifications updated');
}

// Run immediately on page load
pollNotifications();

// Then repeat every 10 seconds
setInterval(pollNotifications, 10000);

/* ── ACCOUNT & NOTIFICATION DROPDOWNS — CLICK TO TOGGLE ─────
   
   What this does:
   - Instead of showing the dropdown on hover (CSS),
     we show it on CLICK and it stays open until
     the user clicks somewhere else
   - This fixes the problem of the dropdown disappearing
     when you move your mouse away
--------------------------------------------------------------- */
document.querySelectorAll('.notification, .account').forEach(item => {
    const btn = item.querySelector('button');
    const dropdown = item.querySelector('.dropdown');

    if (!btn || !dropdown) return;

    // When the button is clicked — toggle the dropdown
    btn.addEventListener('click', (e) => {
        e.stopPropagation(); // prevent the document click from firing immediately

        // Capture visibility BEFORE closing all other dropdowns
        const isVisible = dropdown.style.display === 'block';

        // Close all other dropdowns first
        document.querySelectorAll('.dropdown').forEach(d => {
            d.style.display = 'none';
        });

        // Toggle this one based on its state before the close-all
        dropdown.style.display = isVisible ? 'none' : 'block';
    });
});


/* ── 8. AUTO-SWITCH TO SIGN IN TAB VIA URL QUERY PARAM ──────
   If the URL contains ?tab=signin, automatically switch to
   the Sign In form on the login page.
--------------------------------------------------------------- */
(function () {
    const params = new URLSearchParams(window.location.search);
    const tab = params.get('tab');
    if (tab === 'signin') {
        const signinTab = document.querySelector('.auth-tab[data-tab="signin"]');
        if (signinTab) signinTab.click();
    }
})();


/* ── 9. DELETE JOB — CONFIRMATION MODAL ──────────────────────
   Intercepts all delete-job buttons and shows a confirmation
   modal before actually submitting the form.
--------------------------------------------------------------- */
(function () {
    const modal = document.getElementById('delete-confirm-modal');
    if (!modal) return;

    const overlay   = modal.querySelector('.modal-overlay');
    const cancelBtn = modal.querySelector('.modal-cancel');
    const confirmBtn = modal.querySelector('.modal-confirm');
    const titleSpan = modal.querySelector('.modal-job-title');
    let pendingForm = null;

    function openModal(form, jobTitle) {
        pendingForm = form;
        if (titleSpan) titleSpan.textContent = jobTitle || 'this job';
        modal.classList.add('open');
        modal.setAttribute('aria-hidden', 'false');
    }

    function closeModal() {
        modal.classList.remove('open');
        modal.setAttribute('aria-hidden', 'true');
        pendingForm = null;
    }

    // Intercept every delete button
    document.querySelectorAll('.delete-job-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const form = btn.closest('form');
            const jobTitle = btn.getAttribute('data-job-title') || '';
            openModal(form, jobTitle);
        });
    });

    // Confirm → submit the form
    if (confirmBtn) {
        confirmBtn.addEventListener('click', () => {
            if (pendingForm) pendingForm.submit();
            closeModal();
        });
    }

    // Cancel → close the modal
    if (cancelBtn) cancelBtn.addEventListener('click', closeModal);

    // Clicking the overlay backdrop → close the modal
    if (overlay) overlay.addEventListener('click', closeModal);

    // Escape key → close the modal
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape' && modal.classList.contains('open')) closeModal();
    });
})();


/* ── 10. AUTO-DISMISS TOAST NOTIFICATIONS ────────────────────
   Toast messages fade out and remove themselves after 4 seconds.
--------------------------------------------------------------- */
document.querySelectorAll('.toast-success, .toast-error').forEach(toast => {
    toast.style.transition = 'opacity 0.5s ease';
    setTimeout(() => {
        toast.style.opacity = '0';
        setTimeout(() => toast.remove(), 500);
    }, 4000);
});