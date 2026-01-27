#!/bin/bash

# Script to downsize JPG images from img/ folder
# Scales images to 1080px height, width auto-calculated

for file in img/*.JPG; do
    if [ -f "$file" ]; then
        # Extract filename without extension
        basename="${file%.JPG}"
        # Create output filename
        output="${basename}.small.JPG"
        # Resize to 1080px height, width automatic
        convert "$file" -resize x1080 "$output"
        echo "Processed: $file -> $output"
    fi
done

echo "Done!"
