document.addEventListener('DOMContentLoaded', function() {
    // Toggle entre precios mensuales y anuales
    const pricingToggle = document.getElementById('pricingToggle');
    const monthlyPrices = document.querySelectorAll('.monthly-price');
    const annualPrices = document.querySelectorAll('.annual-price');
    const monthlyPeriods = document.querySelectorAll('.monthly-period');
    const annualPeriods = document.querySelectorAll('.annual-period');
    const annualTotals = document.querySelectorAll('.annual-total');

    if (pricingToggle) {
        pricingToggle.addEventListener('change', function() {
            const isAnnual = this.checked;
            
            monthlyPrices.forEach(price => {
                price.style.display = isAnnual ? 'none' : 'inline';
            });
            
            annualPrices.forEach(price => {
                price.style.display = isAnnual ? 'inline' : 'none';
            });
            
            monthlyPeriods.forEach(period => {
                period.style.display = isAnnual ? 'none' : 'inline';
            });
            
            annualPeriods.forEach(period => {
                period.style.display = isAnnual ? 'inline' : 'none';
            });
            
            annualTotals.forEach(total => {
                total.style.display = isAnnual ? 'block' : 'none';
            });

            // Actualizar campos ocultos del modal
            const inputDuration = document.getElementById('selectedDuration');
            if (inputDuration) inputDuration.value = isAnnual ? 'anual' : 'mensual';
        });
    }

    // Manejar botones de suscripción y mejora
    const subscribeButtons = document.querySelectorAll('.subscribe-btn');
    subscribeButtons.forEach(button => {
        button.addEventListener('click', function() {
            const planName = this.getAttribute('data-plan');
            const isUpgrade = this.getAttribute('data-mejora') === 'true';
            const isAnnual = pricingToggle && pricingToggle.checked;
            
            const pricingCard = this.closest('.pricing-card');
            let price;
            
            if (isAnnual) {
                price = pricingCard.querySelector('.annual-price').textContent;
                const annualTotal = pricingCard.querySelector('.annual-total').textContent;
                document.getElementById('selectedPlanPrice').textContent = `$${price}/mes (${annualTotal})`;
            } else {
                price = pricingCard.querySelector('.monthly-price').textContent;
                document.getElementById('selectedPlanPrice').textContent = `$${price}/mes`;
            }
            
            document.getElementById('selectedPlanName').textContent = planName;
            document.getElementById('selectedPlan').value = planName;
            document.getElementById('selectedDuration').value = isAnnual ? 'anual' : 'mensual';
            document.getElementById('isUpgrade').value = isUpgrade ? 'true' : 'false';

            // Asegurar que el formulario apunte al action correcto
            const form = document.getElementById('subscriptionForm');
            if (form) {
                if (isUpgrade) {
                    form.setAttribute('action', '/Membresia/MejorarPlan');
                    // el backend acepta 'tipoPlan' también
                    const nameAttr = document.getElementById('selectedPlan');
                    if (nameAttr) nameAttr.setAttribute('name', 'tipoPlan');
                } else {
                    form.setAttribute('action', '/Membresia/SuscribirPlan');
                    const nameAttr = document.getElementById('selectedPlan');
                    if (nameAttr) nameAttr.setAttribute('name', 'tipoPlan');
                }
            }
        });
    });

    // Animaciones al hacer scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function(entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-in');
            }
        });
    }, observerOptions);

    document.querySelectorAll('.animate-on-scroll').forEach(el => {
        observer.observe(el);
    });

    // Hover
    const pricingCards = document.querySelectorAll('.pricing-card:not(.popular)');
    pricingCards.forEach(card => {
        card.addEventListener('mouseenter', function() {
            this.style.transform = 'translateY(-8px)';
        });
        
        card.addEventListener('mouseleave', function() {
            this.style.transform = 'translateY(0)';
        });
    });

    // Notificación (demo)
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('success') === 'subscription') {
        showNotification('Suscripción exitosa! Bienvenido a tu nuevo plan.', 'success');
    }

    function showNotification(message, type = 'info') {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show notification-toast`;
        notification.style.cssText = `
            position: fixed;
            top: 100px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notification);
        setTimeout(() => { notification?.remove(); }, 5000);
    }

    // Validación del formulario
    const subscriptionForm = document.querySelector('#subscriptionForm');
    if (subscriptionForm) {
        subscriptionForm.addEventListener('submit', function(e) {
            const plan = document.getElementById('selectedPlan').value;
            const duration = document.getElementById('selectedDuration').value;
            if (!plan || !duration) {
                e.preventDefault();
                showNotification('Por favor selecciona un plan válido.', 'warning');
                return;
            }
            const submitBtn = this.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Procesando...';
            submitBtn.disabled = true;
            setTimeout(() => {
                submitBtn.innerHTML = originalText;
                submitBtn.disabled = false;
            }, 10000);
        });
    }
});

function animateCounter(element, target, duration = 2000) {
    let start = 0;
    const increment = target / (duration / 16);
    function updateCounter() {
        start += increment;
        if (start >= target) {
            element.textContent = target;
        } else {
            element.textContent = Math.floor(start);
            requestAnimationFrame(updateCounter);
        }
    }
    updateCounter();
}