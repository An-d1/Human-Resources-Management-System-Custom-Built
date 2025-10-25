// Professional Navbar Interactions
(function () {
    'use strict';

    // Add scrolled class to navbar on scroll
    const navbar = document.querySelector('.navbar-professional');

    if (navbar) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 50) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        });
    }

    // Set active navigation item based on current page
    function setActiveNavItem() {
        const currentPath = window.location.pathname.toLowerCase();
        const navLinks = document.querySelectorAll('.nav-link-professional');

        navLinks.forEach(link => {
            const linkPath = link.getAttribute('href');

            if (linkPath && currentPath.includes(linkPath.toLowerCase()) && linkPath !== '/') {
                link.classList.add('active');
                link.closest('.nav-item')?.classList.add('active');
            } else if (linkPath === '/' && currentPath === '/') {
                link.classList.add('active');
                link.closest('.nav-item')?.classList.add('active');
            }
        });
    }

    // Close mobile menu when clicking outside
    document.addEventListener('click', function (event) {
        const navbar = document.querySelector('.navbar-professional');
        const navbarCollapse = document.getElementById('navbarContent');
        const navbarToggler = document.querySelector('.navbar-toggler');

        if (navbar && navbarCollapse && navbarToggler) {
            const isClickInside = navbar.contains(event.target);
            const isCollapsed = !navbarCollapse.classList.contains('show');

            if (!isClickInside && !isCollapsed) {
                navbarToggler.click();
            }
        }
    });

    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href !== '#' && href.length > 1) {
                e.preventDefault();
                const target = document.querySelector(href);
                if (target) {
                    const navbarHeight = document.querySelector('.navbar-professional')?.offsetHeight || 0;
                    const targetPosition = target.offsetTop - navbarHeight - 20;

                    window.scrollTo({
                        top: targetPosition,
                        behavior: 'smooth'
                    });

                    // Close mobile menu if open
                    const navbarToggler = document.querySelector('.navbar-toggler');
                    const navbarCollapse = document.getElementById('navbarContent');
                    if (navbarToggler && navbarCollapse?.classList.contains('show')) {
                        navbarToggler.click();
                    }
                }
            }
        });
    });

    // Initialize on page load
    document.addEventListener('DOMContentLoaded', function () {
        setActiveNavItem();

        // Add animation to navbar on page load
        const navbar = document.querySelector('.navbar-professional');
        if (navbar) {
            navbar.style.animation = 'slideDown 0.6s ease-out';
        }
    });

    // Add keyboard navigation support
    document.querySelectorAll('.nav-link-professional').forEach(link => {
        link.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                this.click();
            }
        });
    });

})();

// Add slideDown animation
const style = document.createElement('style');
style.textContent = `
    @keyframes slideDown {
        from {
            transform: translateY(-100%);
            opacity: 0;
        }
        to {
            transform: translateY(0);
            opacity: 1;
        }
    }
`;
document.head.appendChild(style);