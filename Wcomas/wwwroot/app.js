window.getInputValueByQuery = (query) => {
    const el = document.querySelector(query);
    return el ? el.value : "";
};

window.clearInputValueByQuery = (query) => {
    const el = document.querySelector(query);
    if (el) el.value = "";
};

window.getQuillContent = (id) => {
    const el = document.getElementById(id);
    if (!el) return "";
    return el.querySelector('.ql-editor').innerHTML;
};

window.setQuillContent = (id, html) => {
    const el = document.getElementById(id);
    if (!el) return;
    const editor = el.querySelector('.ql-editor');
    if (editor) editor.innerHTML = html || "";
};

// Initialize Quill when component renders
window.initQuill = (id) => {
    if (!document.getElementById(id)) return;
    new Quill('#' + id, {
        theme: 'snow',
        modules: {
            toolbar: [
                [{ 'header': [1, 2, 3, false] }],
                ['bold', 'italic', 'underline', 'strike'],
                ['link', 'blockquote', 'code-block'],
                [{ 'list': 'ordered'}, { 'list': 'bullet' }],
                ['clean']
            ]
        }
    });
};
