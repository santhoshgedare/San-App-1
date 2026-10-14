/** Reads and downsizes an image to a JPEG data URL so API requests stay within size limits. */
export function readImageAsDataUrl(file: File, maxSize = 1280, quality = 0.85): Promise<string> {
  return new Promise((resolve, reject) => {
    if (!file.type.startsWith('image/')) {
      reject(new Error('Please choose an image file.'));
      return;
    }
    if (file.size > 12 * 1024 * 1024) {
      reject(new Error('Choose an image smaller than 12 MB.'));
      return;
    }
    const reader = new FileReader();
    reader.onerror = () => reject(new Error('Could not read the image.'));
    reader.onload = () => {
      const original = reader.result;
      if (typeof original !== 'string') {
        reject(new Error('Could not read the image.'));
        return;
      }
      const image = new Image();
      image.onerror = () => reject(new Error('Could not read the image.'));
      image.onload = () => {
        if (!image.width || !image.height) {
          reject(new Error('The selected image has invalid dimensions.'));
          return;
        }
        const scale = Math.min(1, maxSize / Math.max(image.width, image.height));
        const canvas = document.createElement('canvas');
        canvas.width = Math.round(image.width * scale);
        canvas.height = Math.round(image.height * scale);
        const context = canvas.getContext('2d');
        if (!context) {
          reject(new Error('Could not process the image.'));
          return;
        }
        context.fillStyle = '#fff';
        context.fillRect(0, 0, canvas.width, canvas.height);
        context.drawImage(image, 0, 0, canvas.width, canvas.height);
        try {
          resolve(canvas.toDataURL('image/jpeg', quality));
        } catch {
          reject(new Error('Could not process the image.'));
        }
      };
      image.src = original;
    };
    reader.readAsDataURL(file);
  });
}
