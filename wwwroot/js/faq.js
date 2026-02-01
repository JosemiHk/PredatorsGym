document.addEventListener('DOMContentLoaded', function() {
    // Elementos del DOM
    const searchInput = document.getElementById('faqSearch');
    const clearButton = document.getElementById('clearSearch');
    const categoryButtons = document.querySelectorAll('.category-btn');
    const faqCategories = document.querySelectorAll('.faq-category');
    const faqItems = document.querySelectorAll('.faq-item');
    const resultsInfo = document.getElementById('resultsInfo');
    const noResults = document.getElementById('noResults');
    const helpfulButtons = document.querySelectorAll('.btn-helpful');

    // Variables de estado
    let currentCategory = 'all';
    let searchTerm = '';

    // Inicialización
    init();

    function init() {
        setupEventListeners();
        setupAnimations();
        updateResultsCount();
    }

    function setupEventListeners() {
        // Búsqueda
        if (searchInput) {
            searchInput.addEventListener('input', handleSearch);
            searchInput.addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                }
            });
        }

        if (clearButton) {
            clearButton.addEventListener('click', clearSearch);
        }

        // Navegación por categorías
        categoryButtons.forEach(button => {
            button.addEventListener('click', function() {
                const category = this.getAttribute('data-category');
                setActiveCategory(category);
                filterContent();
            });
        });

        // Botones de utilidad
        helpfulButtons.forEach(button => {
            button.addEventListener('click', handleHelpfulClick);
        });

        // Accordion personalizado
        const accordionButtons = document.querySelectorAll('.accordion-button');
        accordionButtons.forEach(button => {
            button.addEventListener('click', function() {
                // Analítica simple - en producción integrar con Google Analytics
                const question = this.querySelector('.question-text').textContent;
                console.log('FAQ opened:', question);
            });
        });
    }

    function setupAnimations() {
        // Intersection Observer para animaciones
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
    }

    function handleSearch() {
        searchTerm = this.value.toLowerCase().trim();
        
        // Mostrar/ocultar botón de limpiar
        if (clearButton) {
            clearButton.style.display = searchTerm ? 'block' : 'none';
        }

        // Filtrar contenido
        filterContent();
        
        // Highlight de términos de búsqueda
        highlightSearchTerms();
    }

    function clearSearch() {
        searchInput.value = '';
        searchTerm = '';
        clearButton.style.display = 'none';
        filterContent();
        removeHighlights();
        searchInput.focus();
    }

    function setActiveCategory(category) {
        currentCategory = category;
        
        // Actualizar botones activos
        categoryButtons.forEach(btn => {
            btn.classList.remove('active');
        });
        
        const activeButton = document.querySelector(`[data-category="${category}"]`);
        if (activeButton) {
            activeButton.classList.add('active');
        }
    }

    function filterContent() {
        let visibleCount = 0;
        let visibleCategories = 0;

        faqCategories.forEach(category => {
            const categoryName = category.getAttribute('data-category');
            let categoryHasVisibleItems = false;

            // Filtrar por categoría
            const categoryVisible = currentCategory === 'all' || currentCategory === categoryName;

            if (categoryVisible) {
                const items = category.querySelectorAll('.faq-item');
                
                items.forEach(item => {
                    const searchContent = item.getAttribute('data-search-content').toLowerCase();
                    const matchesSearch = !searchTerm || searchContent.includes(searchTerm);

                    if (matchesSearch) {
                        item.classList.remove('hidden');
                        categoryHasVisibleItems = true;
                        visibleCount++;
                    } else {
                        item.classList.add('hidden');
                    }
                });

                if (categoryHasVisibleItems) {
                    category.classList.remove('hidden');
                    visibleCategories++;
                } else {
                    category.classList.add('hidden');
                }
            } else {
                category.classList.add('hidden');
            }
        });

        // Actualizar contador y mostrar/ocultar mensaje de no resultados
        updateResultsCount(visibleCount);
        toggleNoResults(visibleCount === 0);
    }

    function updateResultsCount(count = null) {
        if (!resultsInfo) return;

        if (count === null) {
            count = document.querySelectorAll('.faq-item:not(.hidden)').length;
        }

        const resultText = count === 1 ? 'pregunta encontrada' : 'preguntas encontradas';
        resultsInfo.querySelector('.results-count').textContent = `${count} ${resultText}`;
    }

    function toggleNoResults(show) {
        if (noResults) {
            noResults.style.display = show ? 'block' : 'none';
        }
        
        document.querySelector('.faq-categories').style.display = show ? 'none' : 'block';
    }

    function highlightSearchTerms() {
        if (!searchTerm) return;

        removeHighlights();

        const regex = new RegExp(`(${searchTerm})`, 'gi');
        
        faqItems.forEach(item => {
            if (!item.classList.contains('hidden')) {
                const questionElement = item.querySelector('.question-text');
                const answerElement = item.querySelector('.answer-text');

                if (questionElement) {
                    highlightElement(questionElement, regex);
                }
                if (answerElement) {
                    highlightElement(answerElement, regex);
                }
            }
        });
    }

    function highlightElement(element, regex) {
        const originalText = element.textContent;
        const highlightedText = originalText.replace(regex, '<span class="search-highlight">$1</span>');
        
        if (originalText !== highlightedText) {
            element.innerHTML = highlightedText;
        }
    }

    function removeHighlights() {
        const highlights = document.querySelectorAll('.search-highlight');
        highlights.forEach(highlight => {
            const parent = highlight.parentNode;
            parent.replaceChild(document.createTextNode(highlight.textContent), highlight);
            parent.normalize();
        });
    }

    function handleHelpfulClick() {
        const isHelpful = this.getAttribute('data-helpful') === 'yes';
        const parentActions = this.parentNode;
        
        // Remover estados activos anteriores
        parentActions.querySelectorAll('.btn-helpful').forEach(btn => {
            btn.classList.remove('helpful-yes', 'helpful-no');
        });

        // Agregar nuevo estado
        if (isHelpful) {
            this.classList.add('helpful-yes');
            showNotification('¡Gracias por tu feedback! 👍', 'success');
        } else {
            this.classList.add('helpful-no');
            showNotification('Gracias por tu feedback. Mejoraremos esta respuesta.', 'info');
        }

        // En producción, enviar feedback al servidor
        console.log('Feedback:', isHelpful ? 'helpful' : 'not helpful');
    }

    function showNotification(message, type = 'info') {
        // Crear notificación
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} notification-toast`;
        notification.style.cssText = `
            position: fixed;
            top: 100px;
            right: 20px;
            z-index: 9999;
            min-width: 300px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            border-radius: 8px;
            animation: slideInRight 0.3s ease;
        `;
        
        notification.innerHTML = `
            <div class="d-flex align-items-center">
                <span>${message}</span>
                <button type="button" class="btn-close ms-auto" onclick="this.parentElement.parentElement.remove()"></button>
            </div>
        `;
        
        document.body.appendChild(notification);
        
        // Auto-remove después de 4 segundos
        setTimeout(() => {
            if (notification && notification.parentNode) {
                notification.style.animation = 'slideOutRight 0.3s ease';
                setTimeout(() => notification.remove(), 300);
            }
        }, 4000);
    }

    // Función global para limpiar búsqueda (llamada desde el botón en no-results)
    window.clearSearch = clearSearch;

    // Smooth scroll para enlaces internos
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({ 
                    behavior: 'smooth', 
                    block: 'start',
                    inline: 'nearest'
                });
            }
        });
    });

    // Keyboard shortcuts
    document.addEventListener('keydown', function(e) {
        // Ctrl/Cmd + K para enfocar búsqueda
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            if (searchInput) {
                searchInput.focus();
                searchInput.select();
            }
        }

        // Escape para limpiar búsqueda
        if (e.key === 'Escape' && searchTerm) {
            clearSearch();
        }
    });

    // Analytics de tiempo en página (simple)
    let timeOnPage = 0;
    const startTime = Date.now();
    
    window.addEventListener('beforeunload', function() {
        timeOnPage = Date.now() - startTime;
        console.log('Time on FAQ page:', timeOnPage + 'ms');
    });
});

// CSS para las animaciones de notificación
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
    
    .notification-toast {
        animation: slideInRight 0.3s ease;
    }
`;
document.head.appendChild(style);