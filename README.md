## Background

In a world of "black box" AI, privacy and accuracy are often at odds. Standard Large Language Models (LLMs) suffer from two major flaws: they "hallucinate" facts they don't know, and they require you to send your private data to the cloud to analyze it.

EchoVault was born from a need for a "Private Brain" where a system that stays entirely on your local machine, using Retrieval-Augmented Generation (RAG) to ensure that when you ask a question, the AI answers using your documents as the primary source of truth. 
For example, if the vault says Jupiter has 235 moons, the AI won't argue with its training data; it will respect the Vault.

## The Purpose

The goal of this project is to demonstrate a production-grade approach to local AI orchestration. It isn't just a wrapper for a chatbot; it is a full-lifecycle document management system that handles:

- **Manual Synchronization:** Tracking local file system changes without invasive background services.
- **Vectorized Search:** Converting human language into mathematical vectors for semantic retrieval.
- **Security & Isolation:** Ensuring a multi-user environment where data is strictly siloed behind JWT-secured SQLite vaults.

## Ultilizations

- **Clean Architecture:** To separate domain logic from the specialized Terminal UI (TUI).
- **High-Performance Persistence:** Using `sqlite-vec` for in-process, lightning-fast vector similarity searches.
- **Local Inference:** Leveraging `LLamaSharp` to run GGUF models directly on the CPU/GPU, removing reliance on expensive and privacy-leaking third-party APIs.

## Model Tested and Used

- **all-MiniLM-L6-v2-GGUF:** https://huggingface.co/cstr/all-MiniLM-L6-v2-GGUF (Place the model in C:/Models/)