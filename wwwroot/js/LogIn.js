// Professional Login Page JavaScript
(function () {
    'use strict';

    // Password toggle functionality
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('passwordInput');

    if (togglePassword && passwordInput) {
        togglePassword.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const icon = this.querySelector('i');
            const currentType = passwordInput.getAttribute('type');
            const newType = currentType === 'password' ? 'text' : 'password';

            passwordInput.setAttribute('type', newType);

            // Toggle icon classes
            if (newType === 'text') {
                icon.classList.remove('bi-eye');
                icon.classList.add('bi-eye-slash');
            } else {
                icon.classList.remove('bi-eye-slash');
                icon.classList.add('bi-eye');
            }

            // Add animation effect
            this.style.transform = 'scale(0.9)';
            setTimeout(() => {
                this.style.transform = 'scale(1)';
            }, 150);
        });

        // Prevent button from submitting form
        togglePassword.setAttribute('type', 'button');
    }

    // Form validation feedback
    const form = document.querySelector('.login-form');
    if (form) {
        const inputs = form.querySelectorAll('.form-control-professional');

        inputs.forEach(input => {
            // Add input event listeners for live validation feedback
            input.addEventListener('input', function () {
                if (this.value.trim() !== '') {
                    this.classList.add('has-value');
                } else {
                    this.classList.remove('has-value');
                }
            });

            // Add focus effects
            input.addEventListener('focus', function () {
                this.parentElement.classList.add('focused');
            });

            input.addEventListener('blur', function () {
                this.parentElement.classList.remove('focused');
            });
        });

        // Form submission handling
        form.addEventListener('submit', function (e) {
            const submitBtn = this.querySelector('.btn-login-professional');

            if (submitBtn) {
                // Add loading state
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Signing in...';

                // If validation fails, re-enable button
                setTimeout(() => {
                    if (!form.checkValidity()) {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = '<i class="bi bi-box-arrow-in-right me-2"></i>Sign In';
                    }
                }, 100);
            }
        });
    }

    // Remember me functionality
    const rememberMeCheckbox = document.getElementById('RememberMe');
    const emailInput = document.querySelector('input[name="Email"]');

    if (rememberMeCheckbox && emailInput) {
        // Load saved email if exists
        const savedEmail = localStorage.getItem('rememberedEmail');
        if (savedEmail) {
            emailInput.value = savedEmail;
            rememberMeCheckbox.checked = true;
        }

        // Save email when checkbox is checked
        form?.addEventListener('submit', function () {
            if (rememberMeCheckbox.checked && emailInput.value) {
                localStorage.setItem('rememberedEmail', emailInput.value);
            } else {
                localStorage.removeItem('rememberedEmail');
            }
        });
    }

    // Auto-dismiss alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert-danger-professional');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.3s ease';
            alert.style.opacity = '0';
            setTimeout(() => {
                alert.remove();
            }, 300);
        }, 5000);
    });

    // Add keyboard shortcuts
    document.addEventListener('keydown', function (e) {
        // Ctrl/Cmd + Enter to submit form
        if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
            const submitBtn = document.querySelector('.btn-login-professional');
            if (submitBtn && !submitBtn.disabled) {
                submitBtn.click();
            }
        }
    });

    // Add smooth animations on page load
    window.addEventListener('load', function () {
        const card = document.querySelector('.login-card');
        if (card) {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';

            setTimeout(() => {
                card.style.transition = 'all 0.6s ease-out';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 100);
        }
    });

    // Prevent form resubmission on refresh
    if (window.history.replaceState) {
        window.history.replaceState(null, null, window.location.href);
    }

})();

// Add spinner styles if Bootstrap spinner is used
const style = document.createElement('style');
style.textContent = `
    .spinner-border-sm {
        width: 1rem;
        height: 1rem;
        border-width: 0.15em;
    }
    
    .input-wrapper.focused .form-control-professional {
        border-color: #1e3a8a;
        box-shadow: 0 0 0 4px rgba(30, 58, 138, 0.08);
    }
    
    .form-control-professional.has-value {
        background: white;
    }
`;
document.head.appendChild(style);