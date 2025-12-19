// Script to copy vendor libraries from node_modules to wwwroot/lib
// Cross-platform (works on Windows, Linux, macOS)

const fs = require('fs');
const path = require('path');

// Helper function to create directory recursively
function ensureDir(dir) {
    if (!fs.existsSync(dir)) {
        fs.mkdirSync(dir, { recursive: true });
    }
}

// Helper function to copy file
function copyFile(src, dest) {
    ensureDir(path.dirname(dest));
    fs.copyFileSync(src, dest);
    console.log(`✓ Copied: ${path.basename(dest)}`);
}

// Helper function to copy directory recursively
function copyDir(src, dest) {
    ensureDir(dest);
    const entries = fs.readdirSync(src, { withFileTypes: true });
    
    for (const entry of entries) {
        const srcPath = path.join(src, entry.name);
        const destPath = path.join(dest, entry.name);
        
        if (entry.isDirectory()) {
            copyDir(srcPath, destPath);
        } else {
            fs.copyFileSync(srcPath, destPath);
        }
    }
}

console.log('📦 Copying vendor libraries...\n');

// Copy Vue.js
console.log('Copying Vue.js...');
const vueDir = path.join('wwwroot', 'lib', 'vue');
ensureDir(vueDir);
copyFile(
    path.join('node_modules', 'vue', 'dist', 'vue.global.js'),
    path.join(vueDir, 'vue.global.js')
);
copyFile(
    path.join('node_modules', 'vue', 'dist', 'vue.global.prod.js'),
    path.join(vueDir, 'vue.global.prod.js')
);

// Copy Bootstrap Icons
console.log('\nCopying Bootstrap Icons...');
const bootstrapIconsDir = path.join('wwwroot', 'lib', 'bootstrap-icons');
ensureDir(bootstrapIconsDir);
copyDir(
    path.join('node_modules', 'bootstrap-icons', 'font'),
    path.join(bootstrapIconsDir, 'font')
);
console.log('✓ Copied: bootstrap-icons/font/*');

console.log('\n✅ All libraries copied successfully!');
