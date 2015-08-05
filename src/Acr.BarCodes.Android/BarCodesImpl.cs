﻿using System;
using System.IO;
using ZXing;
using ZXing.Mobile;
using Android.App;
using Android.Graphics;


namespace Acr.BarCodes {

    public class BarCodesImpl : AbstractBarCodesImpl {

        private readonly Func<Activity> getTopActivity;


		public BarCodesImpl(Func<Activity> getTopActivity) {
            this.getTopActivity = getTopActivity;
        }


        protected override MobileBarcodeScanner GetInstance() {
            
            var scanner = new MobileBarcodeScanner();
            if (BarCodes.CustomOverlayFactory != null) {
                var overlay = BarCodes.CustomOverlayFactory();
                if (overlay != null) {
                    scanner.UseCustomOverlay = true;
                    //scanner.CustomOverlay = overlay;
                }
            }
            return scanner;
        }


        protected override Stream ToImageStream(BarcodeWriter writer, BarCodeCreateConfiguration cfg) {
			var stream = new MemoryStream();

			var cf = cfg.ImageType == ImageType.Png
				? Bitmap.CompressFormat.Png
				: Bitmap.CompressFormat.Jpeg;

            var barcode = writer.Write(cfg.BarCode);
            stream.Write(barcode, 0, barcode.Length);            

			stream.Position = 0;
			return stream;
        }
    }
}