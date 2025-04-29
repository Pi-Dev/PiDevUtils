using UnityEngine;
using UnityEditor;
using System.IO;

/* Copyright (c) 2025 Petar Petrov (PeterSvP)
 * https://pi-dev.com * https://store.steampowered.com/pub/pidev
 * 
 * The MIT License (MIT)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 *
 * ============= Description =============
 * Utility for importing textures into Unity as UI-friendly sprites with predefined settings.
 * Configures imported textures for 2D UI use, including pivot adjustment, pixel density, and transparency settings.
 * Automates reimport to immediately apply settings in the editor after import.
 *
 * ============= Usage =============
 * ImporterUtils.ImportAsUITexture("Assets/Path/To/Texture.png");
 */

public static class ImporterUtils
{
    public static void ImportAsUITexture(string relativePath)
    {
        // Import the texture as a Texture2D
        AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;

        if (importer != null)
        {
            // Configure it as a Sprite (UI-friendly)
            importer.textureType = TextureImporterType.Sprite; // For 2D UI
            importer.alphaIsTransparency = true;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 512;
            importer.spritePivot = new Vector2(0.5f, 0f);
            importer.SaveAndReimport(); 
        }
    }
}
