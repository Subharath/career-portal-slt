/* AOS 2.3.1 - Animate On Scroll (MIT License) */
(function (global, factory) {
  typeof exports === 'object' && typeof module !== 'undefined' ? module.exports = factory() :
  typeof define === 'function' && define.amd ? define(factory) :
  (global.AOS = factory());
}(this, (function () { 'use strict';

  var globalConfig = {
    offset: 120,
    delay: 0,
    duration: 400,
    easing: 'ease-in-out',
    once: false,
    mirror: false,
    anchorPlacement: 'top-bottom',
    disableMutationObserver: false,
    startEvent: 'DOMContentLoaded',
    throttleDelay: 99,
    debounceDelay: 50,
    disableScroll: false
  };

  var defaultOptions = globalConfig;

  var AOS = {
    isSupported: function() {
      return 'IntersectionObserver' in window && 'requestAnimationFrame' in window;
    },

    init: function(options) {
      this.options = Object.assign({}, defaultOptions, options);
      this.observerIds = new WeakMap();
      this.elements = [];

      if (!this.isSupported()) {
        console.warn('AOS: IntersectionObserver API is not supported in this browser.');
        return this;
      }

      this.setupObserver();
      this.attachObserver();

      if (this.options.startEvent === 'DOMContentLoaded' && document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => this.refresh());
      } else {
        this.refresh();
      }

      return this;
    },

    setupObserver: function() {
      var self = this;
      var options = {
        threshold: 0,
        rootMargin: this.options.offset + 'px ' + this.options.offset + 'px'
      };

      this.observer = new IntersectionObserver(function(entries) {
        entries.forEach(function(entry) {
          if (entry.isIntersecting) {
            self.triggerAnimation(entry.target);
          } else if (self.options.mirror) {
            self.removeAnimation(entry.target);
          }
        });
      }, options);
    },

    attachObserver: function() {
      var self = this;
      this.elements = document.querySelectorAll('[data-aos]');
      
      this.elements.forEach(function(element) {
        if (!element.classList.contains('aos-animate')) {
          self.observer.observe(element);
        }
      });
    },

    triggerAnimation: function(element) {
      var delay = element.getAttribute('data-aos-delay');
      var duration = element.getAttribute('data-aos-duration');
      var easing = element.getAttribute('data-aos-easing');

      if (delay) element.style.transitionDelay = delay + 'ms';
      if (duration) element.style.transitionDuration = duration + 'ms';
      if (easing) element.style.transitionTimingFunction = easing;

      element.classList.add('aos-animate');

      if (!this.options.once) {
        this.observerIds.set(element, this.observer);
      }
    },

    removeAnimation: function(element) {
      element.classList.remove('aos-animate');
    },

    refresh: function() {
      this.attachObserver();
    },

    refreshHard: function() {
      this.observer.disconnect();
      this.setupObserver();
      this.attachObserver();
    }
  };

  if (typeof document !== 'undefined') {
    var aos = AOS.init();
    
    // Initialize on DOMContentLoaded
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', function() {
        aos.refresh();
      });
    } else {
      aos.refresh();
    }
  }

  return AOS;

})));
