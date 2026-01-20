window.downloadFileFromStream = async (fileName, contentType, streamReference) => {
  const arrayBuffer = await streamReference.arrayBuffer();
  const blob = new Blob([arrayBuffer], { type: contentType });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
};
