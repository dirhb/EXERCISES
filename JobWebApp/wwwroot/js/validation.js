/* ============================================================
   validation.js — client-side form validation (pure JS)
   ------------------------------------------------------------
   All rules run in JavaScript; we add `novalidate` to each managed
   form so the browser's native HTML5 validation never gets in the way.
   Errors are shown inline, right under the field that failed.
   ============================================================ */
(function () {
    const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    // ── small helpers for showing/clearing inline messages ──
    function fieldContainer(input) {
        return input.closest('.auth-field') || input.closest('label') || input.parentElement;
    }

    function showError(input, message) {
        const container = fieldContainer(input);
        let err = container.querySelector('.field-error');
        if (!err) {
            err = document.createElement('small');
            err.className = 'field-error';
            container.appendChild(err);
        }
        err.textContent = message;
        input.classList.add('input-invalid');
    }

    function clearError(input) {
        const container = fieldContainer(input);
        const err = container.querySelector('.field-error');
        if (err) err.remove();
        input.classList.remove('input-invalid');
    }

    // ── reusable test functions ──
    const notEmpty = (v) => v.length > 0;
    const minLen = (n) => (v) => v.length >= n;
    const isEmail = (v) => EMAIL_RE.test(v);
    const isChecked = (v) => v === true;
    const positiveNumber = (v) => v !== '' && !isNaN(v) && parseFloat(v) > 0;
    // Password: at least 8 chars, with a letter and a number.
    const strongPassword = (v) => v.length >= 8 && /[A-Za-z]/.test(v) && /\d/.test(v);

    // Attach a rule set to a single form.
    // Each rule: { selector, test, message, valueOf? }
    function attach(form, rules) {
        if (!form) return;
        form.setAttribute('novalidate', 'novalidate');

        form.addEventListener('submit', (e) => {
            let firstInvalid = null;
            rules.forEach(rule => {
                const input = form.querySelector(rule.selector);
                if (!input) return;
                const value = (input.type === 'checkbox')
                    ? input.checked
                    : (input.value || '').trim();
                if (rule.test(value, form)) {
                    clearError(input);
                } else {
                    showError(input, rule.message);
                    if (!firstInvalid) firstInvalid = input;
                }
            });
            if (firstInvalid) {
                e.preventDefault();
                firstInvalid.focus();
            }
        });

        // Clear a field's error as soon as the user edits it.
        form.addEventListener('input', (e) => {
            if (e.target.matches('input, textarea, select')) clearError(e.target);
        });
        form.addEventListener('change', (e) => {
            if (e.target.matches('input, textarea, select')) clearError(e.target);
        });
    }

    function attachAll(selector, rules) {
        document.querySelectorAll(selector).forEach(form => attach(form, rules));
    }

    // ── 1. REGISTER ──
    attach(document.getElementById('register-form'), [
        { selector: '[name="FirstName"]', test: notEmpty, message: 'Please enter your first name.' },
        { selector: '[name="LastName"]', test: notEmpty, message: 'Please enter your last name.' },
        { selector: '[name="UserName"]', test: minLen(3), message: 'Username must be at least 3 characters.' },
        { selector: '[name="Email"]', test: isEmail, message: 'Please enter a valid email address.' },
        { selector: '[name="Password"]', test: strongPassword, message: 'Password needs 8+ characters, including a letter and a number.' },
        { selector: '.auth-checkbox input[type="checkbox"]', test: isChecked, message: 'You must accept the Terms to continue.' }
    ]);

    // ── 2. SIGN IN ──
    attach(document.getElementById('signin-form'), [
        { selector: '[name="email"]', test: isEmail, message: 'Please enter a valid email address.' },
        { selector: '[name="password"]', test: notEmpty, message: 'Please enter your password.' }
    ]);

    // ── 3. POST A JOB ──
    attach(document.querySelector('form[action="/Employer/PostAJob"]'), [
        { selector: '[name="JobTitle"]', test: minLen(3), message: 'Job title must be at least 3 characters.' },
        { selector: '[name="JobDescription"]', test: minLen(10), message: 'Description must be at least 10 characters.' },
        { selector: '[name="JobType"]', test: notEmpty, message: 'Please choose a job type.' },
        { selector: '[name="GenreID"]', test: notEmpty, message: 'Please choose a genre.' },
        { selector: '[name="Salary"]', test: positiveNumber, message: 'Enter a yearly salary greater than 0.' },
        {
            selector: '#country-search-input',
            test: (v, form) => (form.querySelector('#country-id-hidden').value || '').trim() !== '',
            message: 'Please pick a country from the suggestions.'
        }
    ]);

    // ── 4. OFFER SALARY (one form per applicant) ──
    attachAll('form.salary-form', [
        { selector: '[name="salary"]', test: positiveNumber, message: 'Enter an offer salary greater than 0.' }
    ]);

    // ── 5. ADMIN — BROADCAST NOTIFICATION ──
    attach(document.querySelector('form[action="/Admin/SendNotification"]'), [
        { selector: '[name="text"]', test: notEmpty, message: 'Enter a message to broadcast.' }
    ]);

    // ── 6. ADMIN — NOTIFY SPECIFIC USER (one form per user) ──
    attachAll('form[action="/Admin/SendNotificationToUser"]', [
        { selector: '[name="text"]', test: notEmpty, message: 'Enter a message for this user.' }
    ]);

    // ── 7. REPORT MODAL (target + reason; details optional) ──
    attach(document.querySelector('form[action="/Report/Submit"]'), [
        { selector: '[name="category"]', test: notEmpty, message: 'Please choose a reason.' }
    ]);

    // ── REVIEW (employer rating) ──
    attach(document.getElementById('review-form'), [
        { selector: '[name="rating"]', test: notEmpty, message: 'Please choose a rating.' }
    ]);

    // ── 8. FORGOT PASSWORD ──
    attach(document.getElementById('forgot-form'), [
        { selector: '[name="email"]', test: isEmail, message: 'Please enter a valid email address.' },
        { selector: '[name="username"]', test: notEmpty, message: 'Please enter your username.' },
        { selector: '[name="newPassword"]', test: strongPassword, message: 'Password needs 8+ characters, including a letter and a number.' },
        { selector: '[name="confirmPassword"]', test: (v, form) => v.length > 0 && v === form.querySelector('[name="newPassword"]').value, message: 'Passwords do not match.' }
    ]);

    // ── 9. EDIT PROFILE ──
    attach(document.getElementById('profile-form'), [
        { selector: '[name="firstName"]', test: notEmpty, message: 'Please enter your first name.' },
        { selector: '[name="lastName"]', test: notEmpty, message: 'Please enter your last name.' },
        { selector: '[name="email"]', test: isEmail, message: 'Please enter a valid email address.' }
    ]);

    // ── 10. CHANGE PASSWORD ──
    attach(document.getElementById('changepw-form'), [
        { selector: '[name="currentPassword"]', test: notEmpty, message: 'Enter your current password.' },
        { selector: '[name="newPassword"]', test: strongPassword, message: 'Password needs 8+ characters, including a letter and a number.' },
        { selector: '[name="confirmPassword"]', test: (v, form) => v.length > 0 && v === form.querySelector('[name="newPassword"]').value, message: 'Passwords do not match.' }
    ]);
})();
