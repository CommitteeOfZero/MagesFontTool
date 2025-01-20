namespace FreeType;

interface IReferenceCounted {
	int ReferenceCounter { get; set; }

	void AddReference() {
		if (ReferenceCounter <= 0) {
			throw new InvalidOperationException();
		}
		ReferenceCounter++;
	}

	void RemoveReference() {
		if (ReferenceCounter <= 0) {
			throw new InvalidOperationException();
		}
		if (ReferenceCounter == 1) {
			DisposeCore();
		}
		ReferenceCounter--;
	}

	void DisposeCore();
}
