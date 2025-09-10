// title, content and similar key (via title) validation

let title_is_valid = false;
let current_title = document.querySelector("#title").value;
if (current_title.length == 0) { // new post, enable title validation
    document.querySelector("#title").addEventListener("blur", () => check_title());
} else {
    title_is_valid = true;
}

function test_post_is_valid() {
    if (!title_is_valid) {
        document.querySelector("#post_submit").setAttribute("disabled", "disabled");
        return;
    }
    let title_len = document.querySelector("#title").value.length;
    let content_len = document.querySelector("#content").value.length;
    if (title_len == 0 || content_len == 0 || content_len < 500) {
        document.querySelector("#post_submit").setAttribute("disabled", "disabled");
    } else {
        document.querySelector("#post_submit").removeAttribute("disabled");
    }
}

document.querySelector("#title").addEventListener("keyup", () => test_post_is_valid());
document.querySelector("#content").addEventListener("keyup", () => test_post_is_valid());
test_post_is_valid();

function check_title() {
    let title = document.querySelector("#title").value;
    fetch("/editor/check_title", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: "title=" + encodeURIComponent(title)
    }).then(r => r.text()).then(t => {
        title_is_valid = t !== "true";
        if (title_is_valid) {
            document.querySelector("#title_error").classList.add("hide");
        } else {
            document.querySelector("#title_error").classList.remove("hide");
        }
        test_post_is_valid();
    });
}

// dirty flag to prevent accidentally moving away from the editor

let dirty = true
document.querySelector("#post_submit").addEventListener("click", () => {
    dirty = false;
});
window.addEventListener("beforeunload", e => {
    if (dirty) {
        // note: most browsers ignore this and just detect that I return anything at all
        // if I do (e.g. the below 'confirm' object), then they present their own version of the below
        // I could return 1 here, or true, or even false, and it would trigger a prompt
        e.returnValue = confirm('Are you sure you want to leave?'); // this seems to work on more modern browsers
        return confirm('Are you sure you want to leave?'); // older browsers
    }
    // by returning nothing here, there is no prompt. note that returning false here WOULD trigger a prompt
});

// content uploader

const content = document.querySelector('#content');

content.addEventListener('paste', async (event) => {
    const items = (event.clipboardData || event.originalEvent.clipboardData).items;

    // Find the first supported file in clipboard
    let fileItem = null;
    let fileType = null;

    for (const item of items) {
        if (item.kind === 'file') {
            if (item.type.startsWith('image/')) {
                fileItem = item;
                fileType = 'image';
                break;
            } else if (item.type === 'video/mp4') {
                fileItem = item;
                fileType = 'video';
                break;
            } else if (item.type === 'application/zip' || item.name?.endsWith('.zip')) {
                fileItem = item;
                fileType = 'zip';
                break;
            }
        }
    }

    if (!fileItem) return; // No supported file found

    event.preventDefault();
    const file = fileItem.getAsFile();
    await handle_file_upload(file, fileType);
});

content.addEventListener('dragover', (e) => {
    e.preventDefault();
    content.classList.add('drag-active');
});

content.addEventListener('dragleave', () => {
    content.classList.remove('drag-active');
});

content.addEventListener('drop', async (e) => {
    e.preventDefault();
    content.classList.remove('drag-active');

    const files = e.dataTransfer.files;
    if (files.length === 0) return;

    // Handle each dropped file
    for (const file of files) {
        let fileType = null;

        if (file.type.startsWith('image/')) {
            fileType = 'image';
        } else if (file.type === 'video/mp4') {
            fileType = 'video';
        } else if (file.type === 'application/zip' || file.name.endsWith('.zip')) {
            fileType = 'zip';
        } else {
            continue; // Skip unsupported files
        }

        await handle_file_upload(file, fileType);
    }
});

async function handle_file_upload(fileItem, fileType) {

    // Show name prompt (except for ZIP which uses original filename)
    let name = fileType === 'zip' ? fileItem.name :
        prompt(`Enter a name for this ${fileType} (used for alt text/filename):`,
            fileItem.name || fileType);

    if (name === null) return; // User cancelled

    // Clean filename
    fileName = `${Date.now()}-${name.toLowerCase()
        .replace(/[^a-z0-9.-]/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-|-$/g, '')}`;

    // Ensure correct extension
    if (fileType === 'image' && !fileName.endsWith('.webp') && !fileName.endsWith('.gif')) {
        fileName = fileName.replace(/\.[^.]*$|$/, '.webp');
    } else if (fileType === 'video' && !fileName.endsWith('.mp4')) {
        fileName = fileName.replace(/\.[^.]*$|$/, '.mp4');
    } else if (fileType === 'zip' && !fileName.endsWith('.zip')) {
        fileName = fileName.replace(/\.[^.]*$|$/, '.zip');
    }

    // Create upload indicator
    const originalSelectionStart = content.selectionStart;
    const originalSelectionEnd = content.selectionEnd;
    const uploadIndicator = `[Uploading ${fileType}...]`;

    // Insert at cursor position
    content.value = content.value.substring(0, originalSelectionStart) +
        uploadIndicator +
        content.value.substring(originalSelectionEnd);

    // Disable content during upload
    content.disabled = true;

    try {
        // Upload the file
        const response = await fetch(`/content/${fileName}`, {
            method: 'POST',
            body: fileItem
        });

        if (!response.ok) throw new Error('Upload failed');

        // Generate appropriate markup based on file type
        let markup;
        if (fileType === 'zip') {
            markup = `<a href="/content/${fileName}">${fileName}</a>`;
        } else if (fileType === 'video') {
            markup = `<div align="center"><video width="800" src="/content/${fileName}" controls></video></div>`;
        } else {
            // For images
            markup = `<div align="center"><a href="/content/${fileName}" target="_blank">` +
                `<img style="max-width:800px" src="/content/${fileName}" alt="${name}" />` +
                `</a></div>`;
        }

        // Replace indicator with the generated markup
        content.value = content.value.replace(uploadIndicator, markup);

    } catch (error) {
        console.error('Upload failed:', error);
        // Remove the upload indicator on failure
        content.value = content.value.replace(uploadIndicator, '');
        alert(`${fileType} upload failed. Please try again.`);
    } finally {
        content.disabled = false;
        content.focus();

        // Position cursor at end of inserted content
        setTimeout(() => {
            const newCursorPos = content.value.length;
            content.setSelectionRange(newCursorPos, newCursorPos);
        }, 0);
    }
}