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


/* ── 7. DYNAMIC DISPLAY — REAL NOTIFICATION POLLING —————————─
   Fetches real notifications from the database via the same-origin
   proxy (/Notifications/Get). We can't call the web service directly
   from the browser — it lives on another origin and is plain http,
   so the page's https request would be blocked. The proxy forwards
   the call server-side, exactly like the chat does.
--------------------------------------------------------------- */
function formatNotificationDate(raw) {
    if (!raw) return '';
    const d = new Date(raw);
    if (isNaN(d.getTime())) return '';
    return d.toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
}

/* ── Unread badge helpers ──
   "Read" state is tracked client-side: we remember the highest notification id
   the user has opened the bell on, and count anything newer as unread. */
function notifId(n) {
    return parseInt(n.notificationID || n.NotificationID || 0, 10) || 0;
}

function updateNotifBadge(notifications) {
    const badge = document.getElementById('notif-badge');
    if (!badge) return;
    const maxId = (notifications || []).reduce((m, n) => Math.max(m, notifId(n)), 0);
    badge.dataset.maxId = maxId;
    const lastSeen = parseInt(localStorage.getItem('lastSeenNotifId') || '0', 10) || 0;
    const unread = (notifications || []).filter(n => notifId(n) > lastSeen).length;
    if (unread > 0) {
        badge.textContent = unread > 9 ? '9+' : String(unread);
        badge.style.display = '';
    } else {
        badge.style.display = 'none';
    }
}

function markNotifsRead() {
    const badge = document.getElementById('notif-badge');
    if (!badge) return;
    const maxId = parseInt(badge.dataset.maxId || '0', 10) || 0;
    const lastSeen = parseInt(localStorage.getItem('lastSeenNotifId') || '0', 10) || 0;
    if (maxId > lastSeen) localStorage.setItem('lastSeenNotifId', String(maxId));
    badge.style.display = 'none';
}

async function pollNotifications() {
    const dropdown = document.getElementById('notification-dropdown');
    if (!dropdown) return;

    try {
        const res = await fetch('/Notifications/Get');
        if (!res.ok) throw new Error('Failed to fetch');
        const notifications = await res.json();

        dropdown.innerHTML = '';
        if (notifications && notifications.length > 0) {
            notifications.forEach(n => {
                const li = document.createElement('li');
                const text = n.notificationText || n.NotificationText || '';
                const when = formatNotificationDate(n.notificationDate || n.NotificationDate);

                const msg = document.createElement('div');
                msg.textContent = text;
                li.appendChild(msg);

                if (when) {
                    const time = document.createElement('div');
                    time.textContent = when;
                    time.style.color = 'var(--text-500)';
                    time.style.fontSize = '0.78rem';
                    time.style.marginTop = '2px';
                    li.appendChild(time);
                }
                dropdown.appendChild(li);
            });
        } else {
            const li = document.createElement('li');
            li.textContent = 'No new notifications.';
            li.style.color = 'var(--text-500)';
            dropdown.appendChild(li);
        }

        updateNotifBadge(notifications);
    } catch (err) {
        // Silently ignore if WS is not running; don't spam the console
    }
}

// Run immediately on page load, then repeat every 30 seconds
pollNotifications();
setInterval(pollNotifications, 30000);

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

        // Mark notifications as read when the bell is opened.
        if (!isVisible && item.classList.contains('notification')) markNotifsRead();
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


/* ── 11. CUSTOM FILE INPUT — SHOW FILENAME AFTER PICK ────────
   For any label.custom-file-label, when the hidden input
   changes, update the nearest .custom-file-name span.
--------------------------------------------------------------- */
document.querySelectorAll('.custom-file-label input[type="file"]').forEach(input => {
    input.addEventListener('change', () => {
        const label = input.closest('.custom-file-label');
        if (!label) return;
        // Find the sibling .custom-file-name span
        const nameSpan = label.parentElement.querySelector('.custom-file-name');
        if (!nameSpan) return;
        const defaultText = nameSpan.dataset.default || 'No file';
        nameSpan.textContent = input.files.length > 0
            ? input.files[0].name
            : defaultText;
    });
});

// Resume page specific file name display
const resumeInput = document.getElementById('resume-file-input');
const resumeNameSpan = document.getElementById('resume-file-name');
if (resumeInput && resumeNameSpan) {
    resumeInput.addEventListener('change', () => {
        resumeNameSpan.textContent = resumeInput.files.length > 0
            ? resumeInput.files[0].name
            : 'No file chosen';
    });
}


/* ── 12. JOB HISTORY — GENRE FILTER ──────────────────────────
   When genre checkboxes are clicked, filter the job rows
   shown in the left column by matching data-genre attribute.
--------------------------------------------------------------- */
(function () {
    const genreBoxes = document.querySelectorAll('.genres-list input[name="genre"]');
    const statusBoxes = document.querySelectorAll('input[name="status"]');
    const rows = document.querySelectorAll('.job-row[data-genre]');
    if (!rows.length || (!genreBoxes.length && !statusBoxes.length)) return;

    function checkedValues(boxes) {
        // Ignore empty values (e.g. the "All" radio) so they mean "no filter".
        return Array.from(boxes).filter(b => b.checked).map(b => (b.value || '').toLowerCase()).filter(v => v !== '');
    }

    function applyFilters() {
        const genres = checkedValues(genreBoxes);
        const statuses = checkedValues(statusBoxes);
        rows.forEach(row => {
            // Remember the row's ORIGINAL display (it's flex) so re-showing it
            // restores that instead of clearing it and collapsing the layout.
            if (row.dataset.origDisplay === undefined) {
                row.dataset.origDisplay = row.style.display || '';
            }
            const genreOk = genres.length === 0 || genres.includes((row.dataset.genre || '').toLowerCase());
            const statusOk = statuses.length === 0 || statuses.includes((row.dataset.status || '').toLowerCase());
            row.style.display = (genreOk && statusOk) ? row.dataset.origDisplay : 'none';
        });
    }

    [...genreBoxes, ...statusBoxes].forEach(b => b.addEventListener('change', applyFilters));
})();