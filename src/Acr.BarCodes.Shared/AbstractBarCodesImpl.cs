#if __PLATFORM__
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.Mobile;


namespace Acr.BarCodes {

    public abstract class AbstractBarCodesImpl : IBarCodes {

        protected AbstractBarCodesImpl() {
            var def = MobileBarcodeScanningOptions.Default;

			BarCodeReadConfiguration.Default = new BarCodeReadConfiguration {
				AutoRotate = def.AutoRotate,
				CharacterSet = def.CharacterSet,
				DelayBetweenAnalyzingFrames = def.DelayBetweenAnalyzingFrames,
				InitialDelayBeforeAnalyzingFrames = def.InitialDelayBeforeAnalyzingFrames,
				PureBarcode = def.PureBarcode,
				TryHarder = def.TryHarder,
				TryInverted = def.TryInverted,
				UseFrontCameraIfAvailable = def.UseFrontCameraIfAvailable
            };
        }


		public virtual Stream Create(BarCodeCreateConfiguration cfg) {
#if __ANDROID__
            var writer = new ZXing.BarcodeWriter {
				Format = (BarcodeFormat)Enum.Parse(typeof(BarcodeFormat), cfg.Format.ToString()),
                Encoder = new MultiFormatWriter(),
                Options = new EncodingOptions {
					Height = cfg.Height,
					Margin = cfg.Margin,
					Width = cfg.Height,
					PureBarcode = cfg.PureBarcode
                }
            };
#endif
#if __IOS__
            var writer = new ZXing.Mobile.BarcodeWriter
            {
                Format = (BarcodeFormat)Enum.Parse(typeof(BarcodeFormat), cfg.Format.ToString()),
                Encoder = new MultiFormatWriter(),
                Options = new EncodingOptions
                {
                    Height = cfg.Height,
                    Margin = cfg.Margin,
                    Width = cfg.Height,
                    PureBarcode = cfg.PureBarcode
                }
            };
#endif
            return this.ToImageStream(writer, cfg);
        }


		public async Task<BarCodeResult> Read(BarCodeReadConfiguration config, CancellationToken cancelToken) {
			config = config ?? BarCodeReadConfiguration.Default;
            var scanner = this.GetInstance();
			cancelToken.Register(scanner.Cancel);
            scanner.BottomText = config.BottomText ?? scanner.BottomText;
            scanner.CancelButtonText = config.CancelText ?? scanner.CancelButtonText;
            scanner.FlashButtonText = config.FlashlightText ?? scanner.FlashButtonText;
            scanner.TopText = config.TopText ?? scanner.TopText;

            var result = await scanner.Scan(this.GetXingConfig(config));
            return (result == null || String.IsNullOrWhiteSpace(result.Text)
                ? BarCodeResult.Fail
                : new BarCodeResult(result.Text, FromXingFormat(result.BarcodeFormat))
            );
        }


        private static BarCodeFormat FromXingFormat(ZXing.BarcodeFormat format) {
            return (BarCodeFormat)Enum.Parse(typeof(BarCodeFormat), format.ToString());
        }


		private MobileBarcodeScanningOptions GetXingConfig(BarCodeReadConfiguration cfg) {
            var opts = new MobileBarcodeScanningOptions {
                AutoRotate = cfg.AutoRotate,
                CharacterSet = cfg.CharacterSet,
                DelayBetweenAnalyzingFrames = cfg.DelayBetweenAnalyzingFrames,
                InitialDelayBeforeAnalyzingFrames = cfg.InitialDelayBeforeAnalyzingFrames,
                PureBarcode = cfg.PureBarcode,
                TryHarder = cfg.TryHarder,
                TryInverted = cfg.TryInverted,
                UseFrontCameraIfAvailable = cfg.UseFrontCameraIfAvailable
            };

            if (cfg.Formats != null && cfg.Formats.Count > 0) {
                opts.PossibleFormats = cfg.Formats
                    .Select(x => (BarcodeFormat)(int)x)
                    .ToList();
            }
            return opts;
        }


        
        protected abstract MobileBarcodeScanner GetInstance();
#if __IOS__
        protected abstract Stream ToImageStream(ZXing.Mobile.BarcodeWriter writer, BarCodeCreateConfiguration cfg);
#endif
#if __ANDROID__
        protected abstract Stream ToImageStream(ZXing.BarcodeWriter writer, BarCodeCreateConfiguration cfg);
#endif
    }
}
#endif