/* e:\SLT\JobApp\wwwroot\js\site.js */

// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(window).on('load', function() {
    $('#global-loader').fadeOut('slow');
});

$(document).ready(function() {
    // Show loader on links, but safely ignore Bootstrap UI elements (dropdowns, accordions, tabs)
    $('a').not('[href^="#"]')
          .not('[target="_blank"]')
          .not('[href^="javascript:"]')
          .not('[href=""]') // Ignore empty hrefs
          .not('[data-bs-toggle]') // Ignore bootstrap 5 toggles
          .not('[data-toggle]')    // Ignore older bootstrap toggles
          .not('.dropdown-toggle') // Ignore dropdowns
          .on('click', function(e) {
              // Don't trigger loader if opening in a new tab (Ctrl+Click, Meta+Click)
              if (!e.ctrlKey && !e.metaKey && !e.shiftKey) { 
                  // Don't trigger if default behavior was prevented (like by another script)
                  if (!e.isDefaultPrevented()) {
                      $('#global-loader').fadeIn('fast');
                  }
              }
    });

    // Handle form submissions safely with jQuery Validation
    $('form').on('submit', function() {
        // If client-side validation is active and fails, DON'T show the loader
        if ($(this).valid && !$(this).valid()) {
            return false; 
        }
        $('#global-loader').fadeIn('fast');
    });

    // Fix for navigating back from cached pages (BFCache)
    window.onpageshow = function(event) {
        if (event.persisted) {
            $('#global-loader').hide();
        }
    };

    // Initialize Animate On Scroll (AOS)
    if (typeof AOS !== 'undefined') {
        AOS.init({
            duration: 800,
            once: true,
            offset: 50,
        });
    }

    // Scroll Progress Indicator
    window.addEventListener('scroll', function() {
        let winScroll = document.body.scrollTop || document.documentElement.scrollTop;
        let height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
        let scrolled = (winScroll / height) * 100;
        let progressBar = document.getElementById("scroll-progress-bar");
        if(progressBar) {
            progressBar.style.width = scrolled + "%";
        }
    });
});
