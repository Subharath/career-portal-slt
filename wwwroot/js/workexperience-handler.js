// Work Experience Handler - Service Letter Upload Logic
document.addEventListener('DOMContentLoaded', function() {
    // Handle Job Status change for both work experience entries
    $("select[id$='JobStatus']").on('change', function (ev) {
        var ctrlId = $(this).attr('id');
        var position = ctrlId.search("_");
        var intId = ctrlId.substring(position + 1, position + 2);

        var ctrlEndDateId = "#WorkExperiences_" + intId + "__EndYearMonth";
        var ctrlAttachmentId = "#WorkExperiences_" + intId + "__AttachmentName";

        if ($(this).val() == "Active") {
            // Currently doing job - CANNOT upload service letter
            $(ctrlEndDateId).prop('disabled', true);
            $(ctrlAttachmentId).prop('disabled', true);
            $(ctrlEndDateId).val('');
            $(ctrlAttachmentId).val('');
            
            // Show message to user
            $(ctrlAttachmentId).attr('title', 'Service letters not allowed for active positions');
        }
        else if ($(this).val() == "Inactive") {
            // Left the job - CAN upload service letter  
            $(ctrlEndDateId).prop('disabled', false);
            $(ctrlAttachmentId).prop('disabled', false);
            $(ctrlEndDateId).focus();
            
            $(ctrlAttachmentId).removeAttr('title');
        }
    });

    // Initialize on page load
    $("select[id$='JobStatus']").trigger('change');
});
